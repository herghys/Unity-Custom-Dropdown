using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Herghys.CustomUI.CustomDropdown.Core
{
    public static class CustomDropdownExtension
    {
        /// <summary>
        /// Filter search bar
        /// </summary>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEnumerable<CustomDropdownItem> GetFilteredItemsByCondition(this IEnumerable<CustomDropdownItem> source, Func<CustomDropdownItem, bool> condition = default)
        {
            if (source == null)
                return Enumerable.Empty<CustomDropdownItem>();

            if (condition == null)
                return source;

            return source.Where(condition);
        }

        /// <summary>
        /// Get item keys
        /// </summary>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEnumerable<object> GetSearchBarItemKeyObjects(this IEnumerable<CustomDropdownItem> source, Func<CustomDropdownItem, bool> condition = default)
        {
            return GetFilteredItemsByCondition(source, condition).Select(item => item.Key);
        }

        /// <summary>
        /// Get item keys as string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetSearchBarItemKeysAsString(this IEnumerable<CustomDropdownItem> source, Func<CustomDropdownItem, bool> condition = default)
        {
            return GetFilteredItemsByCondition(source, condition).Select(item => item.Key.ToString());
        }

        public static bool IsParentObjectTypeThatAlsoChild(this CustomDropdownItem customDropdownItem)
        {
            return customDropdownItem.ItemType is (ItemType.Parent) && customDropdownItem.ItemType.HasFlag(ItemType.Child);
        }

        public static bool IsParentObjectType(this CustomDropdownItem customDropdownItem)
        {
            return customDropdownItem.ItemType is (ItemType.Parent) && !customDropdownItem.ItemType.HasFlag(ItemType.Child);
        }

        public static bool IsChildObjectType(this CustomDropdownItem customDropdownItem)
        {
            return customDropdownItem.ItemType is (ItemType.Child) && !customDropdownItem.ItemType.HasFlag(ItemType.Parent);
        }

        public static Func<CustomDropdownItem, bool> IsParentObjectTypeThatAlsoChild()
        {
            return x => x.ItemType is (ItemType.Parent) && x.ItemType.HasFlag(ItemType.Child);
        }

        public static Func<CustomDropdownItem, bool> IsParentObjectType()
        {
            return x => x.ItemType is (ItemType.Parent) && !x.ItemType.HasFlag(ItemType.Child);
        }

        public static Func<CustomDropdownItem, bool> IsChildObjectType()
        {
            return x => x.ItemType is(ItemType.Child) && !x.ItemType.HasFlag(ItemType.Parent);
        }

        public static Func<CustomDropdownItem, bool> IsItemSelected()
        {
            return item => item.IsSelected;
        }

        internal static bool IsFullyActive(this GameObject source)
            => source.activeSelf && source.activeInHierarchy;
    }
}