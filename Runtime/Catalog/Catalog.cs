using System;
using System.Collections.Generic;
using System.Linq;
using Jungle.Attributes;
using UnityEngine;

namespace Jungle.Catalog
{
    [Serializable]
    public class Catalog
    {
        [SerializeReference][JungleClassSelection] private List<CatalogEntry> entries = new();

        private Dictionary<CatalogKey, CatalogEntry> catalogCache;
        private bool cacheNeedsRefresh = true;
        
        /// <summary>
        /// Gets all values associated with the specified key
        /// </summary>
        public IEnumerable<object> GetValues(CatalogKey key)
        {
            EnsureCacheIsValid();

            if (catalogCache.TryGetValue(key, out CatalogEntry values))
            {
               yield return values.Values;
            }

            yield return null;
        }
        

        /// <summary>
        /// Checks if the catalog contains any values for the specified key
        /// </summary>
        public bool HasValues(CatalogKey key)
        {
            return GetValues(key).Any();
        }


        /// <summary>
        /// Clears all entries from the catalog
        /// </summary>
        public void Clear()
        {
            entries.Clear();
            MarkCacheForRefresh();
        }
        

        private void EnsureCacheIsValid()
        {
            if (cacheNeedsRefresh)
            {
                RefreshCache();
            }
        }

        private void RefreshCache()
        {
            catalogCache = new Dictionary<CatalogKey, CatalogEntry>();

            foreach (var entry in entries)
            {
                Debug.Assert(!catalogCache.ContainsKey(entry.Key), $"the Catalog Key {entry.Key} already has an entry, and its values will be overwritten",
                    entry.Key);

                catalogCache[entry.Key] = entry;
            }

            cacheNeedsRefresh = false;
        }

        private void MarkCacheForRefresh()
        {
            cacheNeedsRefresh = true;
        }
    }
}