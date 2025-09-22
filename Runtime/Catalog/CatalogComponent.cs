using UnityEngine;

namespace Jungle.Catalog
{
    public class CatalogComponent : MonoBehaviour
    {
       [SerializeReference] public Catalog catalog=new();
    }
}