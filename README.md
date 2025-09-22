# Jungle Catalog

Runtime catalog utilities for Jungle packages.

## Installation

Use Unity's Package Manager to add the Git URL for this repository or include the folder inside your project `Packages` directory.

This package depends on **Jungle Core**. Ensure `jungle.core` is installed or referenced in your project manifest.

## Usage

1. Create a catalog asset that derives from `CatalogAsset<TItem>`.
2. Implement catalog items by inheriting from `CatalogItem` and providing a unique identifier.
3. Populate the catalog asset in the inspector and access entries at runtime via `CatalogAsset.Get` / `TryGet`.

The runtime API enforces unique identifiers and provides helpful exceptions when configuration is invalid, making catalog data reliable across projects.
