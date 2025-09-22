using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Catalog
{
    /// <summary>
    /// ScriptableObject storing catalog items and providing fast lookup by identifier.
    /// </summary>
    /// <typeparam name="TItem">Type of item stored in the catalog.</typeparam>
    public abstract class CatalogAsset<TItem> : ScriptableObject where TItem : CatalogItem
    {
        [SerializeField]
        private List<TItem> items = new();

        private Dictionary<string, TItem> lookup;

        /// <summary>
        /// Items stored in the catalog.
        /// </summary>
        public IReadOnlyList<TItem> Items => items;

        private void OnEnable()
        {
            RefreshLookup();
        }

        /// <summary>
        /// Rebuilds the lookup dictionary, validating identifiers and duplicates.
        /// </summary>
        public void RefreshLookup()
        {
            lookup = BuildLookup();
        }

        /// <summary>
        /// Returns the catalog item for the provided identifier.
        /// </summary>
        /// <param name="id">Identifier of the item.</param>
        /// <returns>Catalog item associated with the identifier.</returns>
        /// <exception cref="ArgumentException">Thrown when the identifier is null or whitespace.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the identifier is not present.</exception>
        public TItem Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Catalog id must be provided.", nameof(id));
            }

            if (TryGet(id, out var item))
            {
                return item;
            }

            throw new KeyNotFoundException($"Catalog '{name}' does not contain an item with id '{id}'.");
        }

        /// <summary>
        /// Attempts to retrieve an item by identifier.
        /// </summary>
        /// <param name="id">Identifier of the item.</param>
        /// <param name="item">When the method returns, contains the matched item if found.</param>
        /// <returns>True when the identifier exists in the catalog.</returns>
        public bool TryGet(string id, out TItem item)
        {
            if (lookup == null)
            {
                RefreshLookup();
            }

            return lookup.TryGetValue(id, out item);
        }

        private Dictionary<string, TItem> BuildLookup()
        {
            var dictionary = new Dictionary<string, TItem>(items.Count);
            for (var index = 0; index < items.Count; index++)
            {
                var entry = items[index];
                if (entry == null)
                {
                    throw new InvalidOperationException($"Catalog '{name}' contains an unassigned item at index {index}.");
                }

                entry.EnsureConfigured();

                if (dictionary.ContainsKey(entry.Id))
                {
                    throw new InvalidOperationException($"Catalog '{name}' contains duplicate id '{entry.Id}'.");
                }

                dictionary.Add(entry.Id, entry);
            }

            return dictionary;
        }
    }
}
