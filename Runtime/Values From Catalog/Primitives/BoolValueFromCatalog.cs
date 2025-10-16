using System;
using Jungle.Values;
using Jungle.Values.Primitives;

namespace Jungle.Catalog.Values.Primitives
{
    /// <summary>
    /// Bool value implementation that retrieves its value from a Catalog.
    /// </summary>
    [Serializable]
    public class BoolValueFromCatalog : ValueFromCatalog<bool>, IBoolValue
    {
       

        public BoolValueFromCatalog(ICatalogValue catalogValue, CatalogKey catalogKey)
            : base(catalogValue, catalogKey)
        {
        }
    }
}
