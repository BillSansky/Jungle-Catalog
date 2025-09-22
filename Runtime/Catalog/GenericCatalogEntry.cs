using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jungle.Catalog.Implementations
{
    
    public abstract class GenericCatalogEntry<T,T1> : CatalogEntry where T1: IEnumerable<T>
    {
        [SerializeField] private T1 values;       
        
        public override Type AssociatedType => typeof(T);

        public override IEnumerable<object> Values
        {
            get
            {
                foreach (var value in values)
                {
                    yield return value;
                }
            }
        }
    }
}