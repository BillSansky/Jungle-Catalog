using System;
using UnityEngine;

namespace Jungle.Catalog
{
    /// <summary>
    /// Base ScriptableObject for catalog entries.
    /// </summary>
    public abstract class CatalogItem : ScriptableObject, ICatalogItem
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private string displayName = string.Empty;

        /// <inheritdoc />
        public string Id => id;

        /// <inheritdoc />
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? id : displayName;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                id = id.Trim();
            }

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                displayName = displayName.Trim();
            }
        }
#endif

        /// <summary>
        /// Ensures the item identifier is set. Throws if empty to help authoring workflows.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the identifier is empty.</exception>
        public void EnsureConfigured()
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            throw new InvalidOperationException($"Catalog item '{name}' on asset '{GetAssetPath()}' is missing an identifier.");
        }

        private string GetAssetPath()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(this);
#else
            return name;
#endif
        }
    }
}
