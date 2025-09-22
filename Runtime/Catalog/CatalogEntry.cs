using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Jungle.Catalog
{
    [Serializable]
    public abstract class CatalogEntry
    {
        [SerializeField] private CatalogKey key;

        public abstract Type AssociatedType { get; }
        
        public CatalogKey Key 
        { 
            get => key; 
            set => key = value; 
        }
        
        public abstract IEnumerable<object> Values { get; }

       
    }
    
}