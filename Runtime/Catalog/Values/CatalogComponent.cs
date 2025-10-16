using Jungle.Catalog;
using Jungle.Values;
using System;
using UnityEngine;

namespace Jungle.Catalog
{
    public class CatalogComponent : ValueComponent<Catalog>
    {
        [SerializeField]
        private Catalog value;

        public override Catalog Value()
        {
            return value;
        }
    }

    [Serializable]
    public class CatalogValueFromComponent : ValueFromComponent<Catalog, CatalogComponent>, ICatalogValue
    {
    }
}
