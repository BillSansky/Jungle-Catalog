
using Jungle.Values;
using System;
using UnityEngine;

namespace Jungle.Catalog
{
    [CreateAssetMenu(menuName = "Jungle/Values/Custom", fileName = "ValueFromCatalog")]
    public class CatalogAsset : ValueAsset<Catalog>
    {
        [SerializeField]
        private Catalog value;

        public override Catalog Value()
        {
            return value;
        }
    }

    [Serializable]
    public class CatalogValueFromAsset : ValueFromAsset<Catalog, CatalogAsset>, ICatalogValue
    {
    }
}
