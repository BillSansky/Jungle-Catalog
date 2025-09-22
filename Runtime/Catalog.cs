using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Octoputs.Utils
{
    public class Catalog : MonoBehaviour
    {
        [SerializeField] private List<CatalogEntry> entries = new();

        private Dictionary<CatalogKey, List<UnityEngine.Object>> catalogCache;
        private bool cacheNeedsRefresh = true;
        
        /// <summary>
        /// Gets all values associated with the specified key
        /// </summary>
        public IEnumerable<UnityEngine.Object> this[CatalogKey key] => GetValues(key);



        private void Awake()
        {
            RefreshCache();
        }

        private void OnValidate()
        {
            MarkCacheForRefresh();
        }

        /// <summary>
        /// Gets all values associated with the specified key
        /// </summary>
        public IEnumerable<UnityEngine.Object> GetValues(CatalogKey key)
        {
            EnsureCacheIsValid();
            
            if (catalogCache.TryGetValue(key, out List<UnityEngine.Object> values))
            {
                return values.Where(v => v != null);
            }
            
            return Enumerable.Empty<UnityEngine.Object>();
        }


        /// <summary>
        /// Gets the first value associated with the specified key
        /// </summary>
        public UnityEngine.Object GetFirstValue(CatalogKey key)
        {
            return GetValues(key).FirstOrDefault();
        }
        

        /// <summary>
        /// Checks if the catalog contains any values for the specified key
        /// </summary>
        public bool HasValues(CatalogKey key)
        {
            return GetValues(key).Any();
        }

        /// <summary>
        /// Gets all unique keys that have at least one non-null value
        /// </summary>
        public IEnumerable<CatalogKey> GetKeysWithValues()
        {
            EnsureCacheIsValid();
            return catalogCache.Where(kvp => kvp.Value.Any(v => v != null)).Select(kvp => kvp.Key);
        }

        /// <summary>
        /// Adds a value to the specified key
        /// </summary>
        public void AddValue(CatalogKey key, UnityEngine.Object value)
        {
            if (value == null) return;

            var entry = entries.FirstOrDefault(e => e.Key.Equals(key));
            if (entry == null)
            {
                entry = new CatalogEntry(key, value);
                entries.Add(entry);
            }
            else
            {
                entry.Values.Add(value);
            }

            MarkCacheForRefresh();
        }

        /// <summary>
        /// Removes a specific value from the specified key
        /// </summary>
        public bool RemoveValue(CatalogKey key, UnityEngine.Object value)
        {
            var entry = entries.FirstOrDefault(e => e.Key.Equals(key));
            if (entry != null && entry.Values.Remove(value))
            {
                // Remove entry if it has no more values
                if (entry.Values.Count == 0)
                {
                    entries.Remove(entry);
                }

                MarkCacheForRefresh();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes all values associated with the specified key
        /// </summary>
        public bool RemoveKey(CatalogKey key)
        {
            var entry = entries.FirstOrDefault(e => e.Key.Equals(key));
            if (entry != null)
            {
                entries.Remove(entry);
                MarkCacheForRefresh();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the values for the specified key, replacing any existing values
        /// </summary>
        public void SetValues(CatalogKey key, params UnityEngine.Object[] values)
        {
            SetValues(key, (IEnumerable<UnityEngine.Object>)values);
        }

        /// <summary>
        /// Sets the values for the specified key, replacing any existing values
        /// </summary>
        public void SetValues(CatalogKey key, IEnumerable<UnityEngine.Object> values)
        {
            RemoveKey(key);

            var validValues = values?.Where(v => v != null).ToList() ?? new List<UnityEngine.Object>();
            if (validValues.Count > 0)
            {
                var entry = new CatalogEntry(key, validValues);
                entries.Add(entry);
            }

            MarkCacheForRefresh();
        }

        /// <summary>
        /// Clears all entries from the catalog
        /// </summary>
        public void Clear()
        {
            entries.Clear();
            MarkCacheForRefresh();
        }

        /// <summary>
        /// Gets the total number of non-null values across all keys
        /// </summary>
        public int GetTotalValueCount()
        {
            return entries.SelectMany(e => e.Values).Count(v => v != null);
        }

        /// <summary>
        /// Gets the number of values for a specific key
        /// </summary>
        public int GetValueCount(CatalogKey key)
        {
            return GetValues(key).Count();
        }

        /// <summary>
        /// Attempts to convert an object to the target type using component search
        /// </summary>
        public static UnityEngine.Object ConvertToTargetType(UnityEngine.Object obj, Type targetType, ComponentSearchMode searchMode = ComponentSearchMode.First)
        {
            if (obj == null || targetType == null)
                return null;

            // If already the correct type, return as is
            if (targetType.IsAssignableFrom(obj.GetType()))
                return obj;

            // If it's a GameObject, try to find the component
            if (obj is GameObject go)
            {
                return searchMode switch
                {
                    ComponentSearchMode.First => go.GetComponent(targetType),
                    ComponentSearchMode.AllOnObject => go.GetComponents(targetType).FirstOrDefault(),
                    ComponentSearchMode.AllInChildrenAndObject => go.GetComponentInChildren(targetType),
                    _ => null
                };
            }

            // If it's a Component, try to find another component on the same GameObject
            if (obj is Component comp)
            {
                return searchMode switch
                {
                    ComponentSearchMode.First => comp.GetComponent(targetType),
                    ComponentSearchMode.AllOnObject => comp.GetComponents(targetType).FirstOrDefault(),
                    ComponentSearchMode.AllInChildrenAndObject => comp.GetComponentInChildren(targetType),
                    _ => null
                };
            }

            return null;
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
            catalogCache = new Dictionary<CatalogKey, List<UnityEngine.Object>>();
            
            foreach (var entry in entries)
            {
                if (!catalogCache.ContainsKey(entry.Key))
                {
                    catalogCache[entry.Key] = new List<UnityEngine.Object>();
                }
                
                catalogCache[entry.Key].AddRange(entry.Values);
            }
            
            cacheNeedsRefresh = false;
        }

        private void MarkCacheForRefresh()
        {
            cacheNeedsRefresh = true;
        }
    }
}