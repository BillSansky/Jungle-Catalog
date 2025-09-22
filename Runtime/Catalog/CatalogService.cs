using System;
using System.Collections.Generic;

namespace Jungle.Catalog
{
    /// <summary>
    /// Runtime registry for catalog assets. Enables quick access to catalogs without manual wiring.
    /// </summary>
    public static class CatalogService
    {
        private static readonly Dictionary<Type, object> catalogs = new();

        /// <summary>
        /// Registers the provided catalog asset.
        /// </summary>
        /// <typeparam name="TItem">Type of items stored within the catalog.</typeparam>
        /// <param name="catalog">Catalog asset to register.</param>
        public static void Register<TItem>(CatalogAsset<TItem> catalog) where TItem : CatalogItem
        {
            ArgumentNullException.ThrowIfNull(catalog);

            catalogs[typeof(TItem)] = catalog;
        }

        /// <summary>
        /// Attempts to retrieve a registered catalog.
        /// </summary>
        /// <typeparam name="TItem">Type of items stored within the catalog.</typeparam>
        /// <param name="catalog">Retrieved catalog instance when available.</param>
        /// <returns>True if the catalog is registered.</returns>
        public static bool TryGetCatalog<TItem>(out CatalogAsset<TItem> catalog) where TItem : CatalogItem
        {
            if (catalogs.TryGetValue(typeof(TItem), out var stored) && stored is CatalogAsset<TItem> typed)
            {
                catalog = typed;
                return true;
            }

            catalog = null;
            return false;
        }

        /// <summary>
        /// Gets a registered catalog or throws when missing.
        /// </summary>
        /// <typeparam name="TItem">Type of items stored within the catalog.</typeparam>
        /// <returns>The registered catalog asset.</returns>
        public static CatalogAsset<TItem> GetCatalog<TItem>() where TItem : CatalogItem
        {
            if (TryGetCatalog<TItem>(out var catalog))
            {
                return catalog;
            }

            throw new InvalidOperationException($"Catalog for item type '{typeof(TItem).Name}' has not been registered.");
        }

        /// <summary>
        /// Clears all registered catalogs. Useful for unit tests.
        /// </summary>
        public static void Clear()
        {
            catalogs.Clear();
        }
    }
}
