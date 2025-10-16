using System;
using Jungle.Values;
using Jungle.Values.Primitives;

namespace Jungle.Catalog.Values.Primitives
{
    /// <summary>
    /// Int value implementation that retrieves its value from a Catalog.
    /// </summary>
    [Serializable]
    public class IntValueFromCatalog : ValueFromCatalog<int>, IIntValue
    {
        
        public IntValueFromCatalog(ICatalogValue catalogValue, CatalogKey catalogKey)
            : base(catalogValue, catalogKey)
        {
        }
    }
}
