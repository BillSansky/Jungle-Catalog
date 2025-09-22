using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Octoputs.Utils
{
    [Serializable]
    public class CatalogEntry
    {
        [SerializeField] private CatalogKey key;
        [SerializeField] private List<UnityEngine.Object> values = new ();

        public CatalogKey Key 
        { 
            get => key; 
            set => key = value; 
        }
        
        public List<UnityEngine.Object> Values 
        { 
            get => values; 
            set => values = value ?? new List<UnityEngine.Object>(); 
        }

        public CatalogEntry()
        {
        }

        public CatalogEntry(CatalogKey key, params UnityEngine.Object[] values)
        {
            this.key = key;
            this.values = values?.ToList() ?? new List<UnityEngine.Object>();
        }

        public CatalogEntry(CatalogKey key, IEnumerable<UnityEngine.Object> values)
        {
            this.key = key;
            this.values = values?.ToList() ?? new List<UnityEngine.Object>();
        }

        /// <summary>
        /// Gets values cast to the specified type
        /// </summary>
        public IEnumerable<T> GetValuesAs<T>() where T : UnityEngine.Object
        {
            return values.OfType<T>().Where(v => v != null);
        }

        /// <summary>
        /// Gets values cast to the key's associated type
        /// </summary>
        public IEnumerable<UnityEngine.Object> GetValidValues()
        {
            if (key == null)
                return values.Where(v => v != null);
                
            var targetType = key.AssociatedType;
            if (targetType == typeof(UnityEngine.Object))
            {
                return values.Where(v => v != null);
            }

            return values.Where(v => v != null && targetType.IsAssignableFrom(v.GetType()));
        }
    }
    
}