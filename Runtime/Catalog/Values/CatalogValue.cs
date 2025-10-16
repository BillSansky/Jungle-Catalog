using System;
using Jungle.Catalog;
using Jungle.Values;

namespace Jungle.Catalog
{
    public interface ICatalogValue : IValue<Catalog>
    {
    }



    [Serializable]
    public class CatalogValue : LocalValue<Catalog>, ICatalogValue
    {
        public override bool HasMultipleValues => false;
    }
}
