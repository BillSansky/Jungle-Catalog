using System;
using UnityEngine;

namespace Jungle.Catalog
{
    [CreateAssetMenu(fileName = "New Catalog Key", menuName = "Jungle/Catalog Key")]
    public class CatalogKey : ScriptableObject, IEquatable<CatalogKey>
    {
        [SerializeField] private string typeName = "UnityEngine.Object";
        
        private Type cachedType;
        private bool typeCacheValid;

        /// <summary>
        /// Gets the associated type for this catalog key
        /// </summary>
        public Type AssociatedType
        {
            get
            {
                if (!typeCacheValid)
                {
                    RefreshTypeCache();
                }
                return cachedType ?? typeof(UnityEngine.Object);
            }
        }

        /// <summary>
        /// Gets the type name as a string
        /// </summary>
        public string TypeName
        {
            get => typeName;
            set
            {
                if (typeName != value)
                {
                    typeName = value;
                    typeCacheValid = false;
                }
            }
        }

        /// <summary>
        /// Gets the display name for this key (the asset name)
        /// </summary>
        public string DisplayName => name;

        private void OnValidate()
        {
            typeCacheValid = false;
        }

        private void OnEnable()
        {
            typeCacheValid = false;
        }

        private void RefreshTypeCache()
        {
            if (!string.IsNullOrEmpty(typeName))
            {
                cachedType = Type.GetType(typeName);
                if (cachedType == null)
                {
                    // Try to find the type in all loaded assemblies
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        cachedType = assembly.GetType(typeName);
                        if (cachedType != null)
                            break;
                    }
                }
            }
            
            cachedType ??= typeof(UnityEngine.Object);
            typeCacheValid = true;
        }

        /// <summary>
        /// Sets the associated type for this key
        /// </summary>
        public void SetAssociatedType(Type type)
        {
            if (type == null)
            {
                typeName = typeof(UnityEngine.Object).AssemblyQualifiedName;
            }
            else
            {
                typeName = type.AssemblyQualifiedName;
            }
            typeCacheValid = false;
        }

        public bool Equals(CatalogKey other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            // For ScriptableObjects, we compare by instance reference primarily
            // but also by name and type as fallback
            return ReferenceEquals(this, other) || 
                   (name == other.name && typeName == other.typeName);
        }

        public override bool Equals(object obj)
        {
            return obj is CatalogKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, typeName);
        }

        public static bool operator ==(CatalogKey left, CatalogKey right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(CatalogKey left, CatalogKey right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{name} ({AssociatedType?.Name ?? "Object"})";
        }
    }
}