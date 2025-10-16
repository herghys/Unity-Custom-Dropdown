using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Herghys.CustomUI.CustomDropdown.Core
{
    public class CustomDropdownItem : MonoBehaviour
    {
        [SerializeField] ToggleGroup m_ToggleGroupInParent;

        //UI
        [SerializeField, HideInInspector] Toggle m_toggle;
        [SerializeField, HideInInspector] TextMeshProUGUI m_contentText;

        //Is A Child
        [SerializeField, HideInInspector] bool m_hasAnotherItemAsParent;

        //Checker if has childs
        [SerializeField, HideInInspector] bool m_hasChildItem;
        [SerializeField, HideInInspector] CustomDropdownItem m_childItemTemplate;
        [SerializeField, HideInInspector] Transform m_SpawnedChildsContainer;
        [SerializeField, HideInInspector] Graphic m_checkMark;

        //Enable Selection
        [SerializeField, HideInInspector] ToggleSelectionMode m_toggleSelectionMode;
        [SerializeField, HideInInspector] ItemType m_itemType;

        //Additional Toggle
        [SerializeField, HideInInspector] Toggle m_SelectAllToggle;
        [SerializeField, HideInInspector] string m_TotalSubItemSelectedSuffix = "Selected";

        //Graphics
        //Parent Accordion indicator
        [SerializeField, HideInInspector] bool m_showAccordionIndicator;
        [SerializeField, HideInInspector] Image m_dropdownAccordionIndicator;
        [SerializeField, HideInInspector] OpenCloseSpriteState m_accordionSpriteState;

        //Background
        [SerializeField, HideInInspector] bool m_changeGraphicsOnSelected;
        [SerializeField, HideInInspector] Image m_imageToChangeOnSelect;
        [SerializeField, HideInInspector] SearchbarSelectedBackground m_selectionGraphicBlock;

        private List<CustomDropdownItem> m_spawnedChilds = new();
        private CustomDropdownItem m_parent;
        private CustomDropdown _mCustomDropdown;
        private bool m_isSelected;
        private StringBuilder m_contentTextBuilder = new();


        #region Properties
        public int Index { get; private set; } = 0;
        public Guid Guid { get; private set; } = new();
        public Toggle Toggle => m_toggle;
        public Toggle SelectAllToggle => m_SelectAllToggle;
        public bool IsAParentObject => m_hasChildItem && ItemType.HasFlag(ItemType.Parent);
        public bool IsAChildObject => m_hasAnotherItemAsParent && ItemType.HasFlag(ItemType.Child);
        public ToggleSelectionMode SelectionMode => m_toggleSelectionMode;
        public ItemType ItemType => m_itemType;
        public bool IsSelected => m_isSelected;
        public IEnumerable<CustomDropdownItem> SelectedChildren
        {
            get
            {
                if (m_spawnedChilds is null || m_spawnedChilds.Count < 1)
                    return default;

                return this.m_spawnedChilds.GetFilteredItemsByCondition(CustomDropdownExtension.IsItemSelected());
            }
        }

        public bool IsAllChildrenSelected;
        public bool IsOn { get; private set; }

        //Data
        public object Key { get; private set; }
        public IEnumerable<object> Value { get; private set; }
        #endregion

        #region Coroutine
        Coroutine m_toggleChildCoroutine;
        #endregion

        /// <summary>
        /// Unfold if Parent
        /// </summary>
        public void Unfold()
        {
            if (IsAParentObject)
                m_SpawnedChildsContainer.gameObject.SetActive(false);
        }

        /// <summary>
        /// Setup Object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        /// <param name="value"></param>
        /// <exception cref="System.Exception"></exception>
        public void Setup(object key, CustomDropdown customDropdown, int index, bool alreadyExist = false, CustomDropdownItem parent = null, IEnumerable<object> value = default)
        {
            if (IsAChildObject)
            {
                if (parent is null)
                {
                    throw new System.Exception("Please set reference to a parent");
                }
                
                this.m_parent = parent;
            }

            if (IsAParentObject)
            {
                if (value is null || value.Count() < 1)
                {
                    throw new System.Exception("No child objects");
                }
            }

            Index = index;

            if (!alreadyExist)
            {
                Guid = Guid.NewGuid();
                customDropdown.RegisterItem(this);
            }

            if (IsAParentObject)
            {
                m_spawnedChilds = new();
                Value = value;
                
                if (customDropdown.ChildSelectionType == CustomDropdown.SelectionType.Single && m_toggleSelectionMode == ToggleSelectionMode.SelectItem)
                {
                    m_ToggleGroupInParent.allowSwitchOff = true;
                }
                else
                {
                    SetupChild(customDropdown);
                    
                }
                
                m_SpawnedChildsContainer.gameObject.SetActive(false);
            }

            this.Key = key;

            m_contentText.text = key.ToString();
            name = key.ToString();

            _mCustomDropdown = customDropdown;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Setup child objects
        /// </summary>
        private void SetupChild(CustomDropdown customDropdown, IEnumerable<object> childValues = default)
        {
            if (IsAParentObject)
            {
                m_SpawnedChildsContainer.gameObject.SetActive(false);
            }
            if (Value.Count() > m_spawnedChilds.Count)
            {
                var index = 0;
                foreach (var value in Value)
                {
                    var item = Instantiate(m_childItemTemplate, m_SpawnedChildsContainer);
                    item.Setup(value, customDropdown, index, alreadyExist: false, this, childValues);
                    m_spawnedChilds.Add(item);
                    
                    if (customDropdown.ChildSelectionType == CustomDropdown.SelectionType.Single)
                    {
                        item.Toggle.group = m_ToggleGroupInParent;
                        m_ToggleGroupInParent.RegisterToggle(item.Toggle);
                        item.Toggle.isOn = false;
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// Toggle child
        /// </summary>
        /// <param name="value"></param>
        private void ToggleChildUI(bool value)
        {
            m_SpawnedChildsContainer.gameObject.SetActive(value);
            if (m_toggleChildCoroutine != null)
                StopCoroutine(m_toggleChildCoroutine);

            if (!gameObject.IsFullyActive())
                return;

            m_toggleChildCoroutine = StartCoroutine(ToggleChildUIRoutine(value));
        }

        /// <summary>
        /// Toggle child Coroutine
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private IEnumerator ToggleChildUIRoutine(bool value)
        {
            if (m_spawnedChilds is null || m_spawnedChilds.Count == 0)
            {
                yield return _mCustomDropdown.ResizeScrollRect();
                yield break;
            }
            foreach (var child in m_spawnedChilds)
            {
                _mCustomDropdown.MarkContentContainerToRebuild();
                child.gameObject.SetActive(value);
                yield return null;
            }

            yield return _mCustomDropdown.ResizeScrollRect();
        }

        private void ToggleChevronIndicator(bool enabled)
        {
            m_dropdownAccordionIndicator.sprite = enabled ? m_accordionSpriteState.OpenedSprite : m_accordionSpriteState.ClosedSprite;
        }

        private void ChangeBackgroundGraphic(bool enabled)
        {
            m_imageToChangeOnSelect.color = enabled ? m_selectionGraphicBlock.SelectedSprite : m_selectionGraphicBlock.NormlaSprite;
        }

        /// <summary>
        /// [Parent Only]
        /// Toggle Check Mark and Toggle On/Off
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleGraphicObject(bool enabled)
        {
            if (IsAParentObject)
            {
                if (m_toggleSelectionMode == ToggleSelectionMode.ShowChild)
                {
                    m_contentTextBuilder.Clear();
                    var selectedChilds = m_spawnedChilds.Where(child => child.IsSelected);
                    var isAnyChildOn = selectedChilds != null && selectedChilds.Count() > 0;
                    var allChildIsOn = selectedChilds != null && selectedChilds.Count() == m_spawnedChilds.Count;

                    this.IsAllChildrenSelected = allChildIsOn;

                    m_contentTextBuilder.Append(Key);

                    if (isAnyChildOn)
                    {
                        m_contentTextBuilder.Append(" ")
                            .Append($"({selectedChilds.Count()} {m_TotalSubItemSelectedSuffix})");
                    }

                    m_SelectAllToggle.SetIsOnWithoutNotify(allChildIsOn);


                    m_contentText.text = m_contentTextBuilder.ToString();

                    ToggleSelection(isAnyChildOn);
                }
                else
                {
                    m_checkMark.gameObject.SetActive(enabled);   
                    //m_toggle.SetIsOnWithoutNotify(enabled);
                }
            }
        }

        /// <summary>
        /// Toggle Selection from filter
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleSelectionFromFilter(bool enabled)
        {
            gameObject.SetActive(enabled);
            if (m_SpawnedChildsContainer != null)
            {
                m_SpawnedChildsContainer.gameObject.SetActive(enabled);
            }

            if (m_parent != null)
            {
                m_parent.ToggleSelectionFromFilter(enabled);
            }
        }

        #region Event Listeners
        /// <summary>
        /// Toggle All Childs
        /// </summary>
        /// <param name="enabled"></param>
        public void OnSelectAllChilds(bool enabled)
        {
            if (!IsAParentObject)
                return;

            foreach (var child in m_spawnedChilds)
            {
                child.ToggleSelection(enabled, true);
            }
            ToggleGraphicObject(enabled);
        }

        /// <summary>
        /// On Item Selected
        /// </summary>
        /// <param name="enabled"></param>
        public void OnItemSelected(bool enabled)
        {
            if (SelectionMode == (ToggleSelectionMode.ShowChild))
            {
                ToggleChildUI(enabled);
                if (m_showAccordionIndicator)
                {
                    ToggleChevronIndicator(enabled);
                }
            }

            if (SelectionMode == (ToggleSelectionMode.SelectItem))
            {
                if (m_toggle is null)
                    return;

                if (m_changeGraphicsOnSelected)
                {
                    ChangeBackgroundGraphic(enabled);
                }

                ToggleSelection(enabled);
                //m_isSelected = enabled;
                if (m_parent != null && ItemType == ItemType.Child)
                {
                    m_parent.ToggleGraphicObject(enabled);
                }
                else if (ItemType == ItemType.Parent)
                {
                    ToggleGraphicObject(enabled);
                }

                _mCustomDropdown.OnSearchItemSelected();
            }
        }

        /// <summary>
        /// Dispose Item
        /// </summary>
        internal async void DisposeItem()
        {
            if (m_spawnedChilds != null && m_spawnedChilds.Count > 0)
            {
                foreach (var item in m_spawnedChilds)
                {
                    item.DisposeItem();
                    await Task.CompletedTask;
                }
                m_spawnedChilds.Clear();
            }
            await Task.CompletedTask;
            _mCustomDropdown.DisposeItem(this);
            Destroy(gameObject);
        }

        /// <summary>
        /// Toggle Selection
        /// </summary>
        /// <param name="selected"></param>
        internal void ToggleSelection(bool selected, bool includeCheckmark = false)
        {
            m_isSelected = selected;

            if (includeCheckmark)
            {
                if (m_toggle is null)
                    m_checkMark.gameObject.SetActive(selected);
                else
                    m_toggle.isOn = selected;
            }
        }
        #endregion
    }

    [Flags]
    public enum ToggleSelectionMode
    {
        None        = 0,
        ShowChild   = 1 << 0, // 1
        SelectItem  = 1 << 1, // 2
        Everything         = ShowChild | SelectItem // 3
    }

    [Flags]
    public enum ItemType
    {
        None = 0,
        Parent = 1 << 0,
        Child = 2 << 0
    }
}