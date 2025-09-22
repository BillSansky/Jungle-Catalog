using UnityEngine;

namespace Jungle.Catalog
{
    public class CatalogAsset : ScriptableObject
    {
        [SerializeReference] public Catalog catalog;
    }
}