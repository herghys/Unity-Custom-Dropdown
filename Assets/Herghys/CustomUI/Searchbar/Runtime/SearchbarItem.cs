using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Herghys.CustomUI.Searchbar.Runtime;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Herghys.Utility.Searchbar
{
    public class SearchbarItem : MonoBehaviour
    {
        //UI
        [SerializeField, HideInInspector] Toggle m_toggle;
        [SerializeField, HideInInspector] TextMeshProUGUI m_contentText;

        //Is A Child
        [SerializeField, HideInInspector] bool m_hasAnotherItemAsParent;

        //Checker if has childs
        [SerializeField, HideInInspector] bool m_hasChildItem;
        [SerializeField, HideInInspector] SearchbarItem m_childItemTemplate;
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

        private List<SearchbarItem> m_spawnedChilds;
        private SearchbarItem m_parent;
        private Searchbar m_searchbar;
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
        public IEnumerable<SearchbarItem> SelectedChildren
        {
            get
            {
                if (m_spawnedChilds is null || m_spawnedChilds.Count < 1)
                    return default;

                return this.m_spawnedChilds.GetFilteredItemsByCondition(SearchbarExtension.IsItemSelected());
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
        public void Setup(object key, Searchbar searchbar, ToggleGroup toggleGroup, int index, bool alreadyExist = false, SearchbarItem parent = null, IEnumerable<object> value = default)
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
                searchbar.RegisterItem(this);
            }

            if (IsAParentObject)
            {
                m_spawnedChilds = new();
                Value = value;
                SetupChild(searchbar, toggleGroup);

                m_SpawnedChildsContainer.gameObject.SetActive(false);
            }

            this.Key = key;

            m_contentText.text = key.ToString();
            name = key.ToString();

            m_searchbar = searchbar;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Setup child objects
        /// </summary>
        private void SetupChild(Searchbar searchbar, ToggleGroup toggleGroup, IEnumerable<object> childValues = default)
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
                    item.Setup(value, searchbar, toggleGroup, index, alreadyExist: false, this, childValues);
                    m_spawnedChilds.Add(item);
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
            foreach (var child in m_spawnedChilds)
            {
                m_searchbar.MarkContentContainerToRebuild();
                child.gameObject.SetActive(value);
                yield return null;
            }

            yield return m_searchbar.ResizeScrollRect();
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
                m_contentTextBuilder.Clear();
                var selectedChilds = m_spawnedChilds.Where(child => child.IsSelected);
                var isAnyChildOn = selectedChilds != null && selectedChilds.Count() > 0;
                var allChildIsOn = selectedChilds != null && selectedChilds.Count() == m_spawnedChilds.Count;

                this.IsAllChildrenSelected = allChildIsOn;

                m_contentTextBuilder.Append(Key);

                if (isAnyChildOn)
                {
                    m_contentTextBuilder.Append(" ").Append($"({selectedChilds.Count()} {m_TotalSubItemSelectedSuffix})");
                }

                m_SelectAllToggle.SetIsOnWithoutNotify(allChildIsOn);


                m_contentText.text = m_contentTextBuilder.ToString();
                ToggleSelection(isAnyChildOn);
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
                if (m_parent != null)
                {
                    m_parent.ToggleGraphicObject(enabled);
                }

                m_searchbar.OnSearchItemSelected();
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
            m_searchbar.DisposeItem(this);
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
        None = 0,
        ShowChild = 1 << 0,   // 0001
        SelectItem = 2 << 0, // 0010
    }

    [Flags]
    public enum ItemType
    {
        None = 0,
        Parent = 1 << 0,
        Child = 2 << 0
    }
}