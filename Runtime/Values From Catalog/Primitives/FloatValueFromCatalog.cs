using System;
using Jungle.Values;
using Jungle.Values.Primitives;

namespace Jungle.Catalog.Values.Primitives
{
    /// <summary>
    /// Float value implementation that retrieves its value from a Catalog.
    /// </summary>
    [Serializable]
    public class FloatValueFromCatalog : ValueFromCatalog<float>, IFloatValue
    {
      
        public FloatValueFromCatalog(ICatalogValue catalogValue, CatalogKey catalogKey)
            : base(catalogValue, catalogKey)
        {
        }
    }
}
