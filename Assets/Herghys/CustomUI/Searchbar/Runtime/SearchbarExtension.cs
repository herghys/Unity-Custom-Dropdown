using System;
using System.Collections.Generic;
using System.Linq;

namespace Herghys.Utility.Searchbar
{
    public static class SearchbarExtension
    {
        /// <summary>
        /// Filter search bar
        /// </summary>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEnumerable<SearchbarItem> GetFilteredItemsByCondition(this IEnumerable<SearchbarItem> source, Func<SearchbarItem, bool> condition = default)
        {
            if (source == null)
                return Enumerable.Empty<SearchbarItem>();

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
        public static IEnumerable<object> GetSearchBarItemKeyObjects(this IEnumerable<SearchbarItem> source, Func<SearchbarItem, bool> condition = default)
        {
            return GetFilteredItemsByCondition(source, condition).Select(item => item.Key);
        }

        /// <summary>
        /// Get item keys as string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetSearchBarItemKeysAsString(this IEnumerable<SearchbarItem> source, Func<SearchbarItem, bool> condition = default)
        {
            return GetFilteredItemsByCondition(source, condition).Select(item => item.Key.ToString());
        }

        public static bool IsParentObjectTypeThatAlsoChild(this SearchbarItem searchbarItem)
        {
            return searchbarItem.ItemType is (ItemType.Parent) && searchbarItem.ItemType.HasFlag(ItemType.Child);
        }

        public static bool IsParentObjectType(this SearchbarItem searchbarItem)
        {
            return searchbarItem.ItemType is (ItemType.Parent) && !searchbarItem.ItemType.HasFlag(ItemType.Child);
        }

        public static bool IsChildObjectType(this SearchbarItem searchbarItem)
        {
            return searchbarItem.ItemType is (ItemType.Child) && !searchbarItem.ItemType.HasFlag(ItemType.Parent);
        }

        public static Func<SearchbarItem, bool> IsParentObjectTypeThatAlsoChild()
        {
            return x => x.ItemType is (ItemType.Parent) && x.ItemType.HasFlag(ItemType.Child);
        }

        public static Func<SearchbarItem, bool> IsParentObjectType()
        {
            return x => x.ItemType is (ItemType.Parent) && !x.ItemType.HasFlag(ItemType.Child);
        }

        public static Func<SearchbarItem, bool> IsChildObjectType()
        {
            return x => x.ItemType is(ItemType.Child) && !x.ItemType.HasFlag(ItemType.Parent);
        }

        public static Func<SearchbarItem, bool> IsItemSelected()
        {
            return item => item.IsSelected;
        }
    }
}