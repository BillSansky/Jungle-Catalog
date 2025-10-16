using System;
using Jungle.Values;
using Jungle.Values.Primitives;

namespace Jungle.Catalog.Values.Primitives
{
    /// <summary>
    /// String value implementation that retrieves its value from a Catalog.
    /// </summary>
    [Serializable]
    public class StringValueFromCatalog : ValueFromCatalog<string>, IStringValue
    {
        

        public StringValueFromCatalog(ICatalogValue catalogValue, CatalogKey catalogKey)
            : base(catalogValue, catalogKey)
        {
        }
    }
}
