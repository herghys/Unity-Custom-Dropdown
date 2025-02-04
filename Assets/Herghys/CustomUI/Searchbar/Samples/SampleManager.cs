using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Herghys.Utility.Searchbar.Samples
{
    public class SampleManager : MonoBehaviour
    {
        public TextAsset TextAsset;
        public Searchbar SearchBar;

        Dictionary<string, List<object>> chapterData = new();
        void Start()
        {
            var raw = TextAsset.text;
            var chapters = JsonUtility.FromJson<List<ChapterData>>(raw);
            foreach (var chapter in chapters)
            {
                AddToChapterData(chapter.chapter, chapter.subchapter);
            }

            SearchBar.Initialize(chapterData.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<object>)kvp.Value));
        }

        public void AddToChapterData(string key, object value)
        {
            if (!chapterData.ContainsKey(key))
                chapterData.TryAdd(key, new());

            chapterData[key].Add(value);
        }

        [SerializeField] bool autoClose;
        [ContextMenu("Clear Filters")]
        public void ClearFilters()
        {
            SearchBar.ClearFilters(autoClose);
        }
    }

    [Serializable]
    internal class ChapterData
    {
        public string chapter;
        public string subchapter;
    }
}