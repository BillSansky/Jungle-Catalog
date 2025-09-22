#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using Octoputs.DragContext;
using Octoputs.Utils;

namespace Octoputs.Core
{
    [CustomEditor(typeof(Catalog))]
    public class CatalogEditor : UnityEditor.Editor
    {
        private ComponentSearchMode defaultSearchMode = ComponentSearchMode.First;
        private VisualTreeAsset visualTreeAsset;
        private VisualElement root;
        private ScrollView entriesScrollView;
        private Button addEntryButton;
        private EnumField defaultSearchModeField;
        private VisualElement runtimeInfoContainer;
        private Label totalValuesLabel;
        private Label keysWithValuesLabel;

        private void OnEnable()
        {
            string[] guids = AssetDatabase.FindAssets("CatalogEditor t:VisualTreeAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            if (visualTreeAsset != null)
            {
                visualTreeAsset.CloneTree(root);
            }
            else
            {
                // Fallback if UXML is not found
                root.Add(new Label("CatalogEditor.uxml not found"));
                return root;
            }

            // Cache references to UI elements
            entriesScrollView = root.Q<ScrollView>("entries-scroll");
            addEntryButton = root.Q<Button>("add-entry-button");
            defaultSearchModeField = root.Q<EnumField>("default-search-mode");
            runtimeInfoContainer = root.Q<VisualElement>("runtime-info-container");
            totalValuesLabel = root.Q<Label>("total-values-label");
            keysWithValuesLabel = root.Q<Label>("keys-with-values-label");

            // Setup default search mode field
            defaultSearchModeField.Init(defaultSearchMode);
            defaultSearchModeField.RegisterValueChangedCallback(evt => 
            {
                defaultSearchMode = (ComponentSearchMode)evt.newValue;
            });

            // Setup add entry button
            addEntryButton.clicked += AddNewEntry;

            // Bind and refresh
            root.Bind(serializedObject);
            RefreshEntries();
            UpdateRuntimeInfo();

            // Schedule updates for runtime info
            root.schedule.Execute(UpdateRuntimeInfo).Every(100);

            return root;
        }

        private void RefreshEntries()
        {
            entriesScrollView.Clear();

            var entriesProperty = serializedObject.FindProperty("entries");

            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                var entryElement = CreateEntryElement(entriesProperty.GetArrayElementAtIndex(i), i);
                entriesScrollView.Add(entryElement);
            }
        }

        private VisualElement CreateEntryElement(SerializedProperty entryProperty, int index)
        {
            var entryContainer = new VisualElement();
            entryContainer.AddToClassList("octoput-entry-container");

            var keyProperty = entryProperty.FindPropertyRelative("key");
            var valuesProperty = entryProperty.FindPropertyRelative("values");

            // Header
            var headerContainer = new VisualElement();
            headerContainer.AddToClassList("octoput-horizontal-container");

            var catalogKey = keyProperty.objectReferenceValue as CatalogKey;
            var keyName = catalogKey != null ? catalogKey.DisplayName : "No Key";
            var typeName = catalogKey != null ? catalogKey.AssociatedType.Name : "No Type";

            var headerLabel = new Label($"{keyName} ({typeName})");
            headerLabel.AddToClassList("octoput-entry-header");

            var removeButton = new Button(() => RemoveEntry(index)) { text = "Remove" };
            removeButton.AddToClassList("octoput-remove-button");

            headerContainer.Add(headerLabel);
            headerContainer.Add(removeButton);

            // Catalog key field
            var keyField = new PropertyField(keyProperty, "Catalog Key");
            keyField.RegisterValueChangeCallback(evt => 
            {
                RefreshEntries(); // Refresh to update header info
            });

            // Values container
            var valuesContainer = new VisualElement();
            valuesContainer.AddToClassList("octoput-values-container");

            var valuesLabel = new Label("Values:");
            valuesLabel.AddToClassList("octoput-section-label");
            valuesContainer.Add(valuesLabel);

            // Add values
            var associatedType = catalogKey?.AssociatedType ?? typeof(UnityEngine.Object);
            for (int i = 0; i < valuesProperty.arraySize; i++)
            {
                var valueElement = CreateValueElement(valuesProperty.GetArrayElementAtIndex(i), associatedType, i);
                valuesContainer.Add(valueElement);
            }

            // Add value button
            var addValueButton = new Button(() => AddValue(valuesProperty)) { text = "Add Value" };
            addValueButton.AddToClassList("octoput-add-value-button");
            valuesContainer.Add(addValueButton);

            entryContainer.Add(headerContainer);
            entryContainer.Add(keyField);
            entryContainer.Add(valuesContainer);

            return entryContainer;
        }

        private VisualElement CreateValueElement(SerializedProperty valueProperty, Type associatedType, int index)
        {
            var valueContainer = new VisualElement();
            valueContainer.AddToClassList("octoput-horizontal-container");

            var objectField = new ObjectField($"[{index}]");
            objectField.objectType = typeof(UnityEngine.Object);
            objectField.allowSceneObjects = true;
            objectField.BindProperty(valueProperty);

            objectField.RegisterValueChangedCallback(evt => 
            {
                ValidateAndConvertValue(valueProperty, associatedType, evt.newValue, evt.previousValue);
            });

            // Warning icon for type mismatch
            var currentValue = valueProperty.objectReferenceValue;
            if (currentValue != null && !associatedType.IsAssignableFrom(currentValue.GetType()))
            {
                var warningLabel = new Label("⚠");
                warningLabel.AddToClassList("octoput-warning-icon");
                valueContainer.Add(warningLabel);
            }

            var removeButton = new Button(() => RemoveValue(valueProperty)) { text = "-" };
            removeButton.AddToClassList("octoput-remove-value-button");

            valueContainer.Add(objectField);
            valueContainer.Add(removeButton);

            return valueContainer;
        }

        private void ValidateAndConvertValue(SerializedProperty valueProperty, Type associatedType, UnityEngine.Object newValue, UnityEngine.Object previousValue)
        {
            if (newValue != previousValue)
            {
                if (newValue == null)
                {
                    valueProperty.objectReferenceValue = null;
                }
                else if (associatedType.IsAssignableFrom(newValue.GetType()))
                {
                    valueProperty.objectReferenceValue = newValue;
                }
                else
                {
                    var convertedValue = Catalog.ConvertToTargetType(newValue, associatedType, defaultSearchMode);
                    if (convertedValue != null)
                    {
                        valueProperty.objectReferenceValue = convertedValue;
                        EditorUtility.DisplayDialog("Type Conversion", 
                            $"Converted {newValue.GetType().Name} to {convertedValue.GetType().Name}", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Type Mismatch", 
                            $"Cannot convert {newValue.GetType().Name} to {associatedType.Name}", "OK");
                        valueProperty.objectReferenceValue = previousValue;
                    }
                }

                serializedObject.ApplyModifiedProperties();
                RefreshEntries();
            }
        }

        private void AddNewEntry()
        {
            var entriesProperty = serializedObject.FindProperty("entries");
            entriesProperty.arraySize++;
            var newEntry = entriesProperty.GetArrayElementAtIndex(entriesProperty.arraySize - 1);
            InitializeNewEntry(newEntry);
            serializedObject.ApplyModifiedProperties();
            RefreshEntries();
        }

        private void RemoveEntry(int index)
        {
            var entriesProperty = serializedObject.FindProperty("entries");
            entriesProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            RefreshEntries();
        }

        private void AddValue(SerializedProperty valuesProperty)
        {
            valuesProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();
            RefreshEntries();
        }

        private void RemoveValue(SerializedProperty valueProperty)
        {
            var valuesProperty = valueProperty.serializedObject.FindProperty(valueProperty.propertyPath.Substring(0, valueProperty.propertyPath.LastIndexOf('.')));
            var arrayIndex = int.Parse(valueProperty.propertyPath.Substring(valueProperty.propertyPath.LastIndexOf('[') + 1).TrimEnd(']'));
            valuesProperty.DeleteArrayElementAtIndex(arrayIndex);
            serializedObject.ApplyModifiedProperties();
            RefreshEntries();
        }

        private void InitializeNewEntry(SerializedProperty entryProperty)
        {
            var keyProperty = entryProperty.FindPropertyRelative("key");
            keyProperty.objectReferenceValue = null;
        }

        private void UpdateRuntimeInfo()
        {
            if (Application.isPlaying && target is Catalog catalog)
            {
                runtimeInfoContainer.style.display = DisplayStyle.Flex;
                totalValuesLabel.text = $"Total Values: {catalog.GetTotalValueCount()}";
                keysWithValuesLabel.text = $"Keys with Values: {catalog.GetKeysWithValues().Count()}";
            }
            else
            {
                runtimeInfoContainer.style.display = DisplayStyle.None;
            }
        }
    }
}
#endif
