using System;
using System.Collections.Generic;
using System.Linq;
using Jungle.Attributes;
using Jungle.Values;
using UnityEngine;

namespace Jungle.Catalog.Values
{
    
    
    /// <summary>
    /// Base class for values that retrieve their data from a Catalog using a CatalogKey.
    /// </summary>
    /// <typeparam name="T">Type of value being retrieved from the catalog.</typeparam>
    [Serializable]
    public abstract class ValueFromCatalog<T> : IValue<T>
    {
        [SerializeReference][JungleClassSelection] private ICatalogValue catalogValue;
        [SerializeField] private CatalogKey catalogKey;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFromCatalog{T}"/> class with a catalog value and key.
        /// </summary>
        /// <param name="catalogValue">The catalog value provider to retrieve catalogs from.</param>
        /// <param name="catalogKey">The key to use for retrieval.</param>
        public ValueFromCatalog(ICatalogValue catalogValue, CatalogKey catalogKey)
        {
            Debug.Assert(catalogValue != null);
            Debug.Assert(catalogKey != null);

            this.catalogValue = catalogValue;
            this.catalogKey = catalogKey;
        }

        public T Value()
        {
            Debug.Assert(catalogValue != null);
            Debug.Assert(catalogKey != null);

            var catalog = catalogValue.Value();
            Debug.Assert(catalog != null);

            var values = catalog.GetValues(catalogKey);
            var firstValue = values?.FirstOrDefault();

            if (firstValue != null && firstValue is T typedValue)
            {
                return typedValue;
            }

            return default(T);
        }

        public bool HasMultipleValues
        {
            get
            {
                Debug.Assert(catalogValue != null);
                Debug.Assert(catalogKey != null);

                var catalog = catalogValue.Value();
                Debug.Assert(catalog != null);

                var values = catalog.GetValues(catalogKey);
                return values != null && values.Skip(1).Any();
            }
        }

        public IEnumerable<T> Values
        {
            get
            {
                Debug.Assert(catalogValue != null);
                Debug.Assert(catalogKey != null);

                var catalog = catalogValue.Value();
                Debug.Assert(catalog != null);

                var values = catalog.GetValues(catalogKey);
                if (values != null)
                {
                    foreach (var value in values)
                    {
                        if (value is T typedValue)
                        {
                            yield return typedValue;
                        }
                    }
                }
            }
        }

      
        /// <summary>
        /// Gets or sets the catalog key used for value retrieval.
        /// </summary>
        public CatalogKey CatalogKey
        {
            get => catalogKey;
            set => catalogKey = value;
        }
    }
}
