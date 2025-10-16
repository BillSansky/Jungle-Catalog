// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.UIElements;
// using Jungle.Editor;
//
// namespace Jungle.Catalog.Editor
// {
//     [CustomPropertyDrawer(typeof(Catalog))]
//     public class CatalogPropertyDrawer : PropertyDrawer
//     {
//         // Put the real GUIDs of your UXMLs here (Right-click asset -> Copy GUID)
//         private const string TemplateGuid      = "CatalogPropertyDrawer";
//         private const string EntryTemplateGuid = "CatalogPropertyEntry";
//
//         private static VisualTreeAsset rootTemplate;
//         private static VisualTreeAsset entryTemplate;
//
//         public override VisualElement CreatePropertyGUI(SerializedProperty property)
//         {
//             if (property == null) throw new ArgumentNullException(nameof(property));
//
//             var catalogPropertyPath = property.propertyPath;
//             var root = GetRootTemplate().CloneTree();
//
//             ApplyStylingIfAvailable(root);
//
//             var foldout = root.Q<Foldout>("catalog-foldout")
//                 ?? throw new InvalidOperationException("CatalogPropertyDrawer.uxml is missing the 'catalog-foldout' element.");
//
//             foldout.text = property.displayName;
//             foldout.SetValueWithoutNotify(property.isExpanded);
//             foldout.RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);
//
//             var entriesContainer = root.Q<VisualElement>("entries-container")
//                 ?? throw new InvalidOperationException("CatalogPropertyDrawer.uxml is missing the 'entries-container' element.");
//
//             var addButton = root.Q<Button>("add-entry-button")
//                 ?? throw new InvalidOperationException("CatalogPropertyDrawer.uxml is missing the 'add-entry-button' element.");
//
//             var serializedObject = property.serializedObject;
//
//             // >>> Robustly locate the entries array even if Catalog is a [SerializeReference]
//             var entriesProperty = GetEntriesArrayProperty(property)
//                 ?? throw new InvalidOperationException("Catalog property is missing the 'entries' field or it is not an array.");
//
//             var entriesPropertyCopy = entriesProperty.Copy();
//
//             void RefreshEntries()
//             {
//                 serializedObject.Update();
//
//                 // Re-resolve the current Catalog property by path
//                 var currentCatalogProperty = serializedObject.FindProperty(catalogPropertyPath)
//                     ?? throw new InvalidOperationException($"Unable to find catalog property at path '{catalogPropertyPath}'.");
//
//                 var currentEntriesProperty = GetEntriesArrayProperty(currentCatalogProperty)
//                     ?? throw new InvalidOperationException("Catalog property does not contain an 'entries' array.");
//
//                 RebuildEntries(entriesContainer, serializedObject, currentEntriesProperty, RefreshEntries);
//             }
//
//             addButton.clicked += () => ShowAddEntryMenu(serializedObject, entriesProperty.propertyPath, RefreshEntries);
//
//             RefreshEntries();
//             root.TrackPropertyValue(entriesPropertyCopy, _ => RefreshEntries());
//
//             return root;
//         }
//
//         // --- Helper: find "entries" when parent is a SerializedReference or normal object
//         private static SerializedProperty GetEntriesArrayProperty(SerializedProperty catalogProperty)
//         {
//             // 1) Fast path
//             var entries = catalogProperty.FindPropertyRelative("entries");
//             if (entries != null && entries.isArray) return entries;
//
//             // 2) ManagedReference-safe scan of direct children
//             var it = catalogProperty.Copy();
//             var end = catalogProperty.GetEndProperty();
//
//             // Move to first child (if any)
//             bool enterChildren = true;
//             while (it.NextVisible(enterChildren) && !SerializedProperty.EqualContents(it, end))
//             {
//                 enterChildren = false; // after first step, only iterate siblings
//
//                 if (it.depth <= catalogProperty.depth) break; // walked out of this object
//
//                 if (it.name == "entries" && it.isArray)
//                     return it.Copy();
//             }
//
//             return null;
//         }
//
//         private static void ApplyStylingIfAvailable(VisualElement root)
//         {
//             foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
//             {
//                 var stylingType = assembly.GetType("Jungle.Editor.JungleStyling");
//                 if (stylingType == null) continue;
//
//                 var applyMethod = stylingType.GetMethod(
//                     "Apply",
//                     System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
//                     null,
//                     new[] { typeof(VisualElement) },
//                     null);
//                 if (applyMethod == null) continue;
//
//                 applyMethod.Invoke(null, new object[] { root });
//                 return;
//             }
//         }
//
//         private static VisualTreeAsset GetRootTemplate()
//         {
//             if (rootTemplate == null)
//                 rootTemplate = LoadTemplateByGuid(TemplateGuid);
//
//             return rootTemplate;
//         }
//
//         private static VisualTreeAsset GetEntryTemplate()
//         {
//             if (entryTemplate == null)
//                 entryTemplate = LoadTemplateByGuid(EntryTemplateGuid);
//
//             return entryTemplate;
//         }
//
//         private static VisualTreeAsset LoadTemplateByGuid(string guid)
//         {
//             if (string.IsNullOrEmpty(guid))
//                 throw new InvalidOperationException("UXML GUID is empty. Please set the GUID constants in CatalogPropertyDrawer.");
//
//             var pathFromGuid = AssetDatabase.GUIDToAssetPath(guid);
//             if (string.IsNullOrEmpty(pathFromGuid))
//                 throw new InvalidOperationException($"No asset path found for GUID '{guid}'. Is the UXML in the project?");
//
//             var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(pathFromGuid);
//             if (asset == null)
//                 throw new InvalidOperationException($"Failed to load VisualTreeAsset at '{pathFromGuid}' (GUID {guid}).");
//
//             return asset;
//         }
//
//         private void RebuildEntries(
//             VisualElement container,
//             SerializedObject serializedObject,
//             SerializedProperty entriesProperty,
//             Action refreshCallback)
//         {
//             container.Clear();
//
//             for (var index = 0; index < entriesProperty.arraySize; index++)
//             {
//                 var entryProperty = entriesProperty.GetArrayElementAtIndex(index);
//                 container.Add(CreateEntryElement(serializedObject, entriesProperty.propertyPath, entryProperty, index, refreshCallback));
//             }
//         }
//
//         private VisualElement CreateEntryElement(
//             SerializedObject serializedObject,
//             string entriesPropertyPath,
//             SerializedProperty entryProperty,
//             int index,
//             Action refreshCallback)
//         {
//             var element = GetEntryTemplate().CloneTree();
//
//             var headerLabel = element.Q<Label>("entry-label")
//                 ?? throw new InvalidOperationException("CatalogPropertyEntry.uxml is missing the 'entry-label' element.");
//             headerLabel.text = GetEntryLabel(entryProperty);
//
//             var removeButton = element.Q<Button>("remove-entry-button")
//                 ?? throw new InvalidOperationException("CatalogPropertyEntry.uxml is missing the 'remove-entry-button' element.");
//             removeButton.clicked += () =>
//             {
//                 RemoveEntry(serializedObject, entriesPropertyPath, index);
//                 refreshCallback();
//             };
//
//             var contentContainer = element.Q<VisualElement>("entry-content")
//                 ?? throw new InvalidOperationException("CatalogPropertyEntry.uxml is missing the 'entry-content' element.");
//
//             var entryField = new PropertyField(entryProperty) { label = string.Empty };
//             entryField.BindProperty(entryProperty);
//             contentContainer.Add(entryField);
//
//             return element;
//         }
//
//         private void ShowAddEntryMenu(SerializedObject serializedObject, string entriesPropertyPath, Action refreshCallback)
//         {
//             var menu = new GenericMenu();
//             var entryTypes = TypeCache.GetTypesDerivedFrom<CatalogEntry>()
//                 .Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericType && type.GetConstructor(Type.EmptyTypes) != null)
//                 .OrderBy(type => type.FullName)
//                 .ToList();
//
//             if (entryTypes.Count == 0)
//             {
//                 menu.AddDisabledItem(new GUIContent("No catalog entry types found"));
//             }
//             else
//             {
//                 foreach (var entryType in entryTypes)
//                 {
//                     var content = new GUIContent(BuildMenuLabel(entryType));
//                     menu.AddItem(content, false, () =>
//                     {
//                         AddEntry(serializedObject, entriesPropertyPath, entryType);
//                         refreshCallback();
//                     });
//                 }
//             }
//
//             menu.ShowAsContext();
//         }
//
//         private static string BuildMenuLabel(Type type)
//         {
//             if (string.IsNullOrEmpty(type.Namespace))
//                 return type.Name;
//
//             return $"{type.Namespace.Replace('.', '/')}/{type.Name}";
//         }
//
//         private void AddEntry(SerializedObject serializedObject, string entriesPropertyPath, Type entryType)
//         {
//             Undo.RecordObjects(serializedObject.targetObjects, "Add Catalog Entry");
//             serializedObject.Update();
//
//             var entriesProperty = serializedObject.FindProperty(entriesPropertyPath)
//                 ?? throw new InvalidOperationException("Failed to locate entries property when adding a catalog entry.");
//
//             var newIndex = entriesProperty.arraySize;
//             entriesProperty.arraySize++;
//
//             var newElement = entriesProperty.GetArrayElementAtIndex(newIndex);
//             newElement.managedReferenceValue = Activator.CreateInstance(entryType);
//
//             serializedObject.ApplyModifiedProperties();
//         }
//
//         private void RemoveEntry(SerializedObject serializedObject, string entriesPropertyPath, int index)
//         {
//             Undo.RecordObjects(serializedObject.targetObjects, "Remove Catalog Entry");
//             serializedObject.Update();
//
//             var entriesProperty = serializedObject.FindProperty(entriesPropertyPath)
//                 ?? throw new InvalidOperationException("Failed to locate entries property when removing a catalog entry.");
//
//             if (index >= entriesProperty.arraySize)
//                 throw new ArgumentOutOfRangeException(nameof(index), index, "Catalog entry index is out of range.");
//
//             // For SerializedReference elements, Unity often requires two deletes when element is non-null.
//             entriesProperty.DeleteArrayElementAtIndex(index);
//             if (index < entriesProperty.arraySize)
//                 entriesProperty.DeleteArrayElementAtIndex(index);
//
//             serializedObject.ApplyModifiedProperties();
//         }
//
//         private static string GetEntryLabel(SerializedProperty entryProperty)
//         {
//             var fullTypeName = entryProperty.managedReferenceFullTypename;
//             if (string.IsNullOrEmpty(fullTypeName))
//                 return "Unassigned Entry";
//
//             var spaceIndex = fullTypeName.IndexOf(' ');
//             if (spaceIndex >= 0 && spaceIndex + 1 < fullTypeName.Length)
//                 fullTypeName = fullTypeName[(spaceIndex + 1)..];
//
//             var lastDot = fullTypeName.LastIndexOf('.');
//             if (lastDot >= 0 && lastDot + 1 < fullTypeName.Length)
//                 fullTypeName = fullTypeName[(lastDot + 1)..];
//
//             return fullTypeName;
//         }
//     }
// }
