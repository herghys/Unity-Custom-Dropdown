using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Herghys.CustomUI.CustomDropdown.Utilities;

namespace Herghys.CustomUI.CustomDropdown.Core
{
    [RequireComponent(typeof(ToggleGroup))]
    public class CustomDropdown : Selectable
    {
        [Header("Inputs")] 
        [SerializeField] private InputType m_inputType;
        [SerializeField, ShowIf("m_inputType", InputType.InputField)] private TMP_InputField m_inputField;
        [SerializeField, ShowIf("m_inputType", InputType.InputField)] private TextMeshProUGUI m_Placeholder;
        [SerializeField, ShowIf("m_inputType", InputType.Toggle)] private Toggle m_inputToggle;
        [SerializeField, ShowIf("m_inputType", InputType.Toggle)] private TextMeshProUGUI m_inputToggleText;

        [Header("UI Reference")] [SerializeField]
        private ScrollRect m_searchBarScroll;

        [SerializeField] private Transform m_searchBarScrollContentHolder;
        [SerializeField] private float m_maxScrollHeight = 200;

        [SerializeField] private VerticalLayoutGroup m_contentVerticalLayoutGroup;

        //[SerializeField] private GameObject m_applyPanel;
        [SerializeField] private RectTransform m_searchContentRectTransform;
        [SerializeField] private RectTransform m_searchScrollRectTransform;
        [SerializeField] private Button m_cancelButton;

        [Header("Captions")] [SerializeField] private bool includeSubItemsIfAllSelected;
        [SerializeField] private string m_defaultCaption = "No Items Selected";
        [SerializeField] private string m_allSelectedCaption = "All Items Selected";

        [Header("Item Template")]
        [SerializeField] private SelectionType m_childSelectionType;
        [SerializeField] private SelectionType m_parentSelectionType;

        [SerializeField] private ToggleGroup m_parentToggleGroup;
        [SerializeField] private CustomDropdownItem m_itemTemplate;

        [Header("Misc")] [SerializeField] float m_ScrollListTweenTime = 1f;

        private Coroutine m_filterRoutine;
        private Coroutine m_captionRoutine;
        private Coroutine m_clearCoroutine;
        private Coroutine m_resizeRoutine;
        private string m_previousValue = string.Empty;
        private List<GameObject> m_allSearchItems = new();
        private StringBuilder m_captionBuilder = new();

        public static int ParentLimit { get; internal set; } = int.MaxValue;

        #region Properties
        
        public SelectionType ChildSelectionType => m_childSelectionType;
        public SelectionType ParentSelectionType => m_parentSelectionType;
        public Dictionary<Guid, CustomDropdownItem> SpawnedSearchBarItems { get; private set; } = new();
        public string InputValue { get; private set; }
        public bool IsInputEmpty => string.IsNullOrEmpty(InputValue);
        public IEnumerable<CustomDropdownItem> SelectedItems = Array.Empty<CustomDropdownItem>();
        public IEnumerable<CustomDropdownItem> SelectedParents = Array.Empty<CustomDropdownItem>();
        public IEnumerable<CustomDropdownItem> SelectedChildren = Array.Empty<CustomDropdownItem>();

        public IEnumerable<string> SelectedItemsValue
        {
            get
            {
                if (SelectedItems == null)
                    return null;

                return SelectedItems.Select(x => x.Key.ToString());
            }
        }

        public IEnumerable<string> SelectedParentsValue
        {
            get
            {
                if (SelectedParents == null)
                    return null;

                return SelectedParents.Select(x => x.Key.ToString());
            }
        }

        public IEnumerable<string> SelectedChildrenValue
        {
            get
            {
                if (SelectedChildren == null)
                    return null;
                return SelectedChildren.Select(x => x.Key.ToString());
            }
        }

        #endregion

        public UnityEvent OnFiltersCleared = new();

        #region Unity

        protected override void OnEnable()
        {
            base.OnEnable();
            m_searchBarScroll.gameObject.SetActive(false);
            switch (m_inputType)
            {
                case InputType.InputField:
                    RegisterInputField();
                    break;
                case InputType.Toggle:
                    RegisterToggle();
                    break;
            }
        }

        protected override void OnDisable()
        {
            switch (m_inputType)
            {
                case InputType.InputField:
                    UnregisterInputField();
                    break;
                case InputType.Toggle:
                    UnregisterToggle();
                    break;
            }
            base.OnDisable();
           
        }

        protected virtual void RegisterInputField()
        {
            m_inputToggle?.gameObject.SetActive(false);
            m_inputField?.gameObject.SetActive(true);
            
            m_inputField?.onValueChanged?.AddListener(OnInputValueChanged);
            m_inputField?.onEndEdit?.AddListener(OnInputValueEndEdit);
            m_inputField?.onSelect?.AddListener(OnSelected);

        }
        
        protected virtual void UnregisterInputField()
        {
            m_inputField?.onValueChanged.RemoveListener(OnInputValueChanged);
            m_inputField?.onEndEdit.RemoveListener(OnInputValueEndEdit);
            m_inputField?.onSelect.RemoveListener(OnSelected);
        }
        
        protected virtual void RegisterToggle()
        {
            m_inputField?.gameObject.SetActive(false);
            m_inputToggle?.gameObject.SetActive(true);

            m_inputToggle?.onValueChanged.AddListener(OnSelected);
        }
        
        protected virtual void UnregisterToggle()
        {
            m_inputToggle?.onValueChanged.RemoveListener(OnSelected);
        }

        #endregion

        /// <summary>
        /// Initialize by Dictionary
        /// </summary>
        /// <param name="values"></param>
        /// <exception cref="System.Exception"></exception>
        public void Initialize(Dictionary<string, IEnumerable<object>> values)
        {
            if (values is null || values.Count() < 1)
                return;

            if (m_itemTemplate is null)
                throw new System.Exception("Template is null");

            SpawnItems(values);
            BuildPlaceholder(true);
            SetupSearchObjects();
        }

        /// <summary>
        /// Initialize by dictionary of array string
        /// </summary>
        /// <param name="values"></param>
        /// <exception cref="System.Exception"></exception>
        public void Initialize(Dictionary<string, string[]> values)
        {
            if (values is null || values.Count() < 1)
                return;

            if (m_itemTemplate is null)
                throw new System.Exception("Template is null");

            SpawnItems(values.ToDictionary(x => x.Key, y => (IEnumerable<object>)y.Value));
            BuildPlaceholder(true);
            SetupSearchObjects();
        }

        /// <summary>
        /// Spawn Items
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="contents"></param>
        private void SpawnItems<K>(Dictionary<K, IEnumerable<object>> contents)
        {
            var index = 0;
            if (ParentSelectionType == SelectionType.Single)
            {
                m_parentToggleGroup.allowSwitchOff = true;
            }
            foreach (var content in contents)
            {
                var item = Instantiate(m_itemTemplate, m_searchBarScrollContentHolder);
                item.Setup(content.Key, this, index, false, value: content.Value);
                
                if (ParentSelectionType == SelectionType.Single)
                {
                    item.Toggle.group = m_parentToggleGroup;
                    m_parentToggleGroup.RegisterToggle(item.Toggle);
                    item.Toggle.isOn = false;
                }
                index++;
            }
        }

        /// <summary>
        /// Set View Limit By Index
        /// </summary>
        /// <param name="viewLimit">If view limit < 0 = No Limit</param>
        public void SetViewLimit(int viewLimit = -1)
        {
            CustomDropdown.ParentLimit = viewLimit < 0 ? int.MaxValue : viewLimit;
        }

        /// <summary>
        /// Set View Limit By Parent Object Key
        /// </summary>
        /// <param name="key"></param>
        public void SetViewLimit(string key = "")
        {
            var limitIndex = -1;

            if (string.IsNullOrEmpty(key))
            {
                var limit = SpawnedSearchBarItems.Values.FirstOrDefault(item => item.Key.ToString() == key);
                limitIndex = limit.Index;
            }

            SetViewLimit(limitIndex);
        }

        /// <summary>
        /// Start Filter
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="checkEmpty"></param>
        void StartFilter(string inputValue = "", bool checkEmpty = true, bool previousValueEqualityCheck = true,
            bool isInitialCheck = false)
        {
            if (inputValue.Equals(m_previousValue) && previousValueEqualityCheck)
                return;

            m_previousValue = string.IsNullOrEmpty(InputValue) ? string.Empty : InputValue;
            InputValue = string.IsNullOrEmpty(inputValue) ? string.Empty : inputValue;

            if (!gameObject.activeSelf)
                return;

            if (!checkEmpty)
                m_searchBarScroll.gameObject.SetActive(true);

            //m_applyPanel.SetActive(true);

            if (m_filterRoutine != null)
                StopCoroutine(m_filterRoutine);

            if (!gameObject.IsFullyActive())
                return;

            m_filterRoutine = StartCoroutine(FilterItems(checkEmpty = false, isInitialCheck));
        }

        /// <summary>
        /// Hide Filter List
        /// </summary>
        public void HideFilterList()
        {
            m_inputField.text = string.Empty;

            m_searchBarScroll.gameObject.SetActive(false);
            m_cancelButton.gameObject.SetActive(false);

            //m_applyPanel.SetActive(false);
        }

        /// <summary>
        /// Register SearchItem
        /// </summary>
        /// <param name="item"></param>
        public void RegisterItem(CustomDropdownItem item)
        {
            SpawnedSearchBarItems.TryAdd(item.Guid, item);
        }

        public void DisposeItem(CustomDropdownItem item)
        {
            SpawnedSearchBarItems.Remove(item.Guid);
        }

        /// <summary>
        /// Clear and reset CustomDropdown Spawned Items
        /// </summary>
        public async void ClearAndReset()
        {
            foreach (var item in new List<CustomDropdownItem>(
                         SpawnedSearchBarItems.Values.Where(CustomDropdownExtension.IsParentObjectType)))
            {
                item.DisposeItem();
                await System.Threading.Tasks.Task.CompletedTask;
            }

            SpawnedSearchBarItems.Clear();
        }

        #region Item Selection

        /// <summary>
        /// On SearchItem Selected
        /// </summary>
        public void OnSearchItemSelected()
        {
            if (SpawnedSearchBarItems != null)
            {
                SelectedItems = SpawnedSearchBarItems.Values.GetFilteredItemsByCondition(item => item.IsSelected);
                SelectedParents = SelectedItems.GetFilteredItemsByCondition(CustomDropdownExtension.IsParentObjectType());
                SelectedChildren = SelectedItems.GetFilteredItemsByCondition(CustomDropdownExtension.IsChildObjectType());
            }

            StartResizeScrollRect();
            BuildPlaceholder();
        }
        #endregion

        #region UI Utility

        /// <summary>
        /// Modify Default Caption (Non Selected Items)
        /// </summary>
        /// <param name="caption"></param>
        public void ModifyDefaultCaption(string caption)
            => m_defaultCaption = caption;

        /// <summary>
        /// Modify All Selected Items Caption
        /// </summary>
        /// <param name="caption"></param>
        public void ModifyAllSelectedCaption(string caption)
            => m_allSelectedCaption = caption;

        /// <summary>
        /// Clear Filters
        /// </summary>
        /// <param name="autoClose">Check wether you need to auto close the filtered list</param>
        public void ClearFilters(bool autoClose = true)
        {
            if (m_clearCoroutine != null)
                StopCoroutine(m_clearCoroutine);

            if (!gameObject.IsFullyActive())
                return;

            m_clearCoroutine = StartCoroutine(IE_ClearFilters(autoClose));
        }

        /// <summary>
        /// Clear Filters routine for smoother UI
        /// </summary>
        /// <param name="autoClose">Check wether you need to auto close the filtered list</param>
        /// <returns></returns>
        IEnumerator IE_ClearFilters(bool autoClose)
        {
            if (SpawnedSearchBarItems != null)
            {
                foreach (var item in SpawnedSearchBarItems.Values)
                {
                    item.ToggleSelection(false, true);

                    if (item.IsParentObjectType())
                        item.Unfold();

                    yield return ResizeScrollRect();
                }
            }

            SelectedItems = default;
            SelectedParents = default;
            SelectedChildren = default;

            m_inputField.SetTextWithoutNotify(string.Empty);

            yield return ResizeScrollRect();
            if (autoClose)
            {
                m_searchBarScroll.gameObject.SetActive(false);
            }

            EventSystem.current.SetSelectedGameObject(gameObject);
            BuildPlaceholder();

            OnFiltersCleared?.Invoke();
        }

        /// <summary>
        /// Build placeholder
        /// </summary>
        /// <param name="initial"></param>
        public void BuildPlaceholder(bool initial = false)
        {
            if (m_captionRoutine != null)
                StopCoroutine(m_captionRoutine);

            if (!gameObject.IsFullyActive())
                return;

            m_captionRoutine = StartCoroutine(IE_BuildPlaceholder(initial));
        }

        /// <summary>
        /// Build placeholder coroutine
        /// </summary>
        /// <param name="initial"></param>
        /// <returns></returns>
        private IEnumerator IE_BuildPlaceholder(bool initial = false)
        {
            yield return null;
            if (initial || SelectedItems is null || SelectedItems.Count() < 1)
            {
                SetEmptyPlaceholderCaption();
            }
            else
            {
                BuildPlaceholderCaption();
            }

            SetPlaceholderLabel(m_captionBuilder.ToString().TrimEnd());
        }

        /// <summary>
        /// Set empty placeholder
        /// </summary>
        public void SetEmptyPlaceholderCaption()
        {
            m_captionBuilder.Clear();
            m_captionBuilder.Append(m_defaultCaption);
            
            SetPlaceholderLabel(m_captionBuilder.ToString().TrimEnd());
        }

        /// <summary>
        /// Build placeholder caption
        /// </summary>
        private void BuildPlaceholderCaption()
        {
            m_captionBuilder.Clear();
            if (SelectedItems.Count() == SpawnedSearchBarItems.Count)
            {
                m_captionBuilder.Append(m_allSelectedCaption);
                return;
            }

            for (int i = 0; i < SelectedParents.Count(); i++)
            {
                var item = SelectedParents.ElementAt(i);
                var value = item.Key.ToString();
                m_captionBuilder.Append(value);

                var isAllSelected = item.IsAllChildrenSelected;

                if (isAllSelected && !includeSubItemsIfAllSelected)
                {
                    if (i < SelectedParents.Count() - 1)
                        m_captionBuilder.Append(", ");
                    continue;
                }
                

                var selectedChildren = item.SelectedChildren;
                if (selectedChildren is null || selectedChildren.Count() < 1)
                {
                    if (i < SelectedParents.Count() - 1)
                        m_captionBuilder.Append(", ");  
                    continue;
                }
                m_captionBuilder.Append(": ");

                for (int j = 0; j < selectedChildren.Count(); j++)
                {
                    var childItem = selectedChildren.ElementAt(j);
                    var childValue = childItem.Key.ToString();

                    if (j < selectedChildren.Count() - 1)
                        m_captionBuilder.Append(childValue).Append(", ");
                    else
                        m_captionBuilder.Append(childValue);
                }

                if (i < SelectedParents.Count() - 1)
                    m_captionBuilder.Append("; ");
            }
        }

        private void SetPlaceholderLabel(string text)
        {
            if (m_Placeholder)
                m_Placeholder.text = m_captionBuilder.ToString().TrimEnd();
            
            if (m_inputToggleText)
                m_inputToggleText.text = m_captionBuilder.ToString().TrimEnd();
        }
        
        
        /// <summary>
        /// Mark content container for rebuikd
        /// </summary>
        public void MarkContentContainerToRebuild()
        {
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)m_searchBarScrollContentHolder);
        }

        /// <summary>
        /// Force rebuild content container
        /// </summary>
        public void RebuildContentContainer()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)m_searchBarScrollContentHolder);
        }

        /// <summary>
        /// Start Resize
        /// </summary>
        internal void StartResizeScrollRect()
        {
            if (m_resizeRoutine != null)
                StopCoroutine(m_resizeRoutine);

            if (!gameObject.IsFullyActive())
                return;

            m_resizeRoutine = StartCoroutine(ResizeScrollRect());
        }

        /// <summary>
        /// Resize Input Scroll rect to match max height / content size
        /// </summary>
        /// <returns></returns>
        public IEnumerator ResizeScrollRect()
        {
            var contentHeight = m_searchContentRectTransform.rect.height;
            var newHeight = m_maxScrollHeight;

            MarkContentContainerToRebuild();
            if (contentHeight < m_maxScrollHeight)
            {
                newHeight = MathF.Min(contentHeight, m_maxScrollHeight);
                m_searchScrollRectTransform.sizeDelta = new Vector2(m_searchScrollRectTransform.rect.width, newHeight);
            }
            else
            {
                m_searchScrollRectTransform.sizeDelta =
                    new Vector2(m_searchScrollRectTransform.rect.width, m_maxScrollHeight);
            }

            while (m_searchScrollRectTransform.rect.height < newHeight)
            {
                m_searchScrollRectTransform.sizeDelta = Vector2.Lerp(a: m_searchContentRectTransform.sizeDelta,
                    b: new Vector2(m_searchScrollRectTransform.rect.width, newHeight), m_ScrollListTweenTime);
                yield return null;
            }

            m_searchScrollRectTransform.sizeDelta = new Vector2(m_searchScrollRectTransform.rect.width, newHeight);

            yield return null;
            RebuildContentContainer();
        }

        #endregion

        #region Filter Utility

        /// <summary>
        /// Setup search object
        /// </summary>
        private void SetupSearchObjects()
        {
            var filtered = GetFilteredGameObject(gameObject, true, x => !x.name.Contains("Template")).Distinct();
            m_allSearchItems.AddRange(filtered);
        }

        /// <summary>
        /// Get filtered game object by condition
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="includeMe"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        IEnumerable<GameObject> GetFilteredGameObject(GameObject obj, bool includeMe, Func<GameObject, bool> condition)
        {
            HashSet<GameObject> filtered = new();
            GetChildRecursive(obj, includeMe, ref filtered, condition);
            return filtered;
        }

        /// <summary>
        /// Filter items coroutine
        /// </summary>
        /// <param name="checkEmpty"></param>
        /// <returns></returns>
        private IEnumerator FilterItems(bool checkEmpty, bool isInitialCheck = false)
        {
            if (checkEmpty)
            {
                if (IsInputEmpty && m_searchBarScroll.gameObject.activeSelf)
                {
                    m_searchBarScroll.gameObject.SetActive(false);
                }
                else if (!IsInputEmpty && !m_searchBarScroll.gameObject.activeSelf)
                {
                    m_searchBarScroll.gameObject.SetActive(true);
                }
            }
            else
            {
                m_searchBarScroll.gameObject.SetActive(true);
            }

            var filteredItems = GetFilteredItemsByInput();

            if (filteredItems is null)
                yield break;

            bool removedParentExceedingLimit = false;

            //Remove Parents That Exceed View Limit
            IEnumerator RemoveAllParentsExceedingViewLimit()
            {
                var parents =
                    SpawnedSearchBarItems.Values.GetFilteredItemsByCondition(CustomDropdownExtension.IsParentObjectType);

                foreach (var parent in parents)
                {
                    if (parent.Index >= CustomDropdown.ParentLimit)
                    {
                        MarkContentContainerToRebuild();
                        parent.ToggleSelectionFromFilter(false);
                    }

                    yield return null;
                    yield return ResizeScrollRect();
                }
            }

            if (!m_previousValue.Equals(InputValue) || (string.IsNullOrEmpty(InputValue) && !isInitialCheck))
            {
                var exclusions = SpawnedSearchBarItems.Values.Except(filteredItems);

                if (exclusions != null)
                {
                    foreach (var item in exclusions)
                    {
                        MarkContentContainerToRebuild();
                        item.ToggleSelectionFromFilter(false);
                        yield return null;
                        yield return ResizeScrollRect();
                    }
                }

                removedParentExceedingLimit = true;
                foreach (var item in filteredItems)
                {
                    MarkContentContainerToRebuild();
                    if (item.ItemType == ItemType.Parent)
                    {
                        if (item.Index >= CustomDropdown.ParentLimit)
                        {
                            item.ToggleSelectionFromFilter(false);
                        }
                        else
                        {
                            item.ToggleSelectionFromFilter(true);
                        }
                    }
                    else
                    {
                        MarkContentContainerToRebuild();
                        item.ToggleSelectionFromFilter(true);
                    }

                    yield return null;
                    yield return ResizeScrollRect();
                }
            }

            if (!removedParentExceedingLimit)
                yield return RemoveAllParentsExceedingViewLimit();

            yield return ResizeScrollRect();
            RebuildContentContainer();
        }

        /// <summary>
        /// Filter by input
        /// </summary>
        /// <returns></returns>
        private IEnumerable<CustomDropdownItem> GetFilteredItemsByInput()
        {
            if (IsInputEmpty)
            {
                return this.SpawnedSearchBarItems.Values.GetFilteredItemsByCondition(CustomDropdownExtension
                    .IsParentObjectType());
            }

            var filteredItems = SpawnedSearchBarItems
                .Where(pair => pair.Value.Key.ToString().ToLower().Contains(InputValue.ToLower()))
                .Select(pair => pair.Value);

            return filteredItems;
        }

        #endregion

        #region Misc Utility

        /// <summary>
        /// Get Childs
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="includeMe"></param>
        /// <param name="collection"></param>
        /// <param name="condition"></param>
        private void GetChildRecursive(GameObject obj, bool includeMe, ref HashSet<GameObject> collection,
            Func<GameObject, bool> condition)
        {
            if (null == obj)
                return;

            if (!condition(obj))
                return;

            if (includeMe)
                collection.Add(obj);

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                    continue;

                if (!condition(child.gameObject))
                    continue;

                collection.Add(child.gameObject);
                GetChildRecursive(child.gameObject, false, ref collection, condition);
            }
        }

        #endregion

        #region Event Listeners

        /// <summary>
        /// On Input selected
        /// </summary>
        /// <param name="value"></param>
        public void OnSelected(string value)
        {
            m_cancelButton.gameObject.SetActive(true);
            StartFilter(value, checkEmpty: false, false, true);
        }

        /// <summary>
        /// On Input Selected As Toggle
        /// </summary>
        /// <param name="active"></param>
        public void OnSelected(bool active)
        {
            m_searchBarScroll.gameObject.SetActive(active);
            if (!active)
                return;
            
            OnSelected(string.Empty);
        }

        /// <summary>
        /// On Input Deselected (perhaps)
        /// </summary>
        /// <param name="value"></param>
        public void OnDeselected(string value)
        {
            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;

            if (currentSelectedGameObject is null)
            {
                HideFilterList();
                return;
            }

            bool isSearchItems = m_allSearchItems.Contains(currentSelectedGameObject);

            if (!isSearchItems)
            {
                HideFilterList();
            }
        }

        /// <summary>
        /// On Input value changed
        /// </summary>
        /// <param name="value"></param>
        public void OnInputValueChanged(string value)
        {
            StartFilter(value);
        }

        /// <summary>
        /// On Input value end edit
        /// </summary>
        /// <param name="value"></param>
        public void OnInputValueEndEdit(string value)
        {
            StartFilter(value);
        }

        #endregion

        public enum SelectionType
        {
            Single,
            Multiple
        }

        #region UNITY EDITOR

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (Application.isPlaying)
                return;

            base.OnValidate();

            if (m_inputField is null)
                m_inputField = GetComponentInChildren<TMP_InputField>();

            if (m_Placeholder is null)
                m_Placeholder = m_inputField.transform.GetChild(0).Find("Placeholder").GetComponent<TextMeshProUGUI>();

            if (m_searchBarScroll is null)
                m_searchBarScroll = GetComponentInChildren<ScrollRect>(true);

            if (m_searchBarScrollContentHolder is null)
                m_searchBarScrollContentHolder = m_searchBarScroll.transform.GetChild(0)
                    .GetComponentInChildren<VerticalLayoutGroup>(true).transform;

            if (m_contentVerticalLayoutGroup is null)
                m_contentVerticalLayoutGroup = m_searchBarScrollContentHolder.GetComponent<VerticalLayoutGroup>();

            // if (m_childToggleGroup is null)
            //     m_childToggleGroup = GetComponent<ToggleGroup>();

            if (m_searchContentRectTransform is null && m_searchBarScrollContentHolder != null)
                m_searchContentRectTransform = (RectTransform)m_searchBarScrollContentHolder.transform;

            if (m_searchScrollRectTransform is null && m_searchBarScroll != null)
                m_searchScrollRectTransform = (RectTransform)m_searchBarScroll.transform;

            if (m_cancelButton is null && m_inputField != null)
                m_cancelButton = m_inputField.GetComponentInChildren<Button>(true);
        }

        protected override void Reset()
        {
            base.Reset();

            m_inputField = GetComponentInChildren<TMP_InputField>();
            m_Placeholder = m_inputField.transform.GetChild(0).Find("Placeholder").GetComponent<TextMeshProUGUI>();
            m_searchBarScroll = GetComponentInChildren<ScrollRect>(true);
            m_searchBarScrollContentHolder = m_searchBarScroll.transform.GetChild(0)
                .GetComponentInChildren<VerticalLayoutGroup>(true).transform;
            m_contentVerticalLayoutGroup = m_searchBarScrollContentHolder.GetComponent<VerticalLayoutGroup>();
            // m_childToggleGroup = GetComponent<ToggleGroup>();

            if (m_searchBarScrollContentHolder != null)
                m_searchContentRectTransform = (RectTransform)m_searchBarScrollContentHolder.transform;

            if (m_searchBarScroll != null)
                m_searchScrollRectTransform = (RectTransform)m_searchBarScroll.transform;

            if (m_inputField != null)
                m_cancelButton = m_inputField.GetComponentInChildren<Button>(true);
        }
#endif

        #endregion
    }
    
    public enum InputType
    {
        InputField, Toggle
    }
}