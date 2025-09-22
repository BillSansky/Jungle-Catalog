namespace Jungle.Catalog
{
    /// <summary>
    /// Base contract for catalog entries. Items stored in a catalog must expose a unique identifier.
    /// </summary>
    public interface ICatalogItem
    {
        /// <summary>
        /// Unique identifier of the item within the catalog.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Optional friendly name displayed in tooling.
        /// </summary>
        string DisplayName { get; }
    }
}
