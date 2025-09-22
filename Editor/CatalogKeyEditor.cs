#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using Octoputs.Utils;

namespace Octoputs.Core
{
    [CustomEditor(typeof(CatalogKey))]
    public class CatalogKeyEditor : UnityEditor.Editor
    {
        private VisualTreeAsset visualTreeAsset;
        private VisualElement root;
        private TextField typeDisplayField;
        private Button selectTypeButton;
        private VisualElement typeInfoContainer;
        private Label fullNameLabel;
        private Label assemblyLabel;
        private Label baseTypeLabel;

        private void OnEnable()
        {
            string[] guids = AssetDatabase.FindAssets("CatalogKeyEditor t:VisualTreeAsset");
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
                root.Add(new Label("CatalogKeyEditor.uxml not found"));
                return root;
            }

            // Cache references to UI elements
            typeDisplayField = root.Q<TextField>("type-display-field");
            selectTypeButton = root.Q<Button>("select-type-button");
            typeInfoContainer = root.Q<VisualElement>("type-info-container");
            fullNameLabel = root.Q<Label>("full-name-label");
            assemblyLabel = root.Q<Label>("assembly-label");
            baseTypeLabel = root.Q<Label>("base-type-label");

            // Setup button click handler
            selectTypeButton.clicked += () => ShowTypeSelectionMenu((CatalogKey)target);

            // Bind and update UI
            root.Bind(serializedObject);
            UpdateUI();

            // Schedule periodic updates
            root.schedule.Execute(UpdateUI).Every(100);

            return root;
        }

        private void UpdateUI()
        {
            var catalogKey = (CatalogKey)target;
            var currentType = catalogKey.AssociatedType;
            var typeDisplayName = currentType?.Name ?? "Unknown Type";

            // Update type display
            typeDisplayField.value = typeDisplayName;

            // Update type info
            if (currentType != null && currentType != typeof(UnityEngine.Object))
            {
                typeInfoContainer.style.display = DisplayStyle.Flex;
                fullNameLabel.text = $"Full Name: {currentType.FullName}";
                assemblyLabel.text = $"Assembly: {currentType.Assembly.GetName().Name}";

                if (currentType.BaseType != null && currentType.BaseType != typeof(object))
                {
                    baseTypeLabel.text = $"Base Type: {currentType.BaseType.Name}";
                    baseTypeLabel.style.display = DisplayStyle.Flex;
                }
                else
                {
                    baseTypeLabel.style.display = DisplayStyle.None;
                }
            }
            else
            {
                typeInfoContainer.style.display = DisplayStyle.None;
            }
        }

        private void ShowTypeSelectionMenu(CatalogKey catalogKey)
        {
            var menu = new GenericMenu();

            // Get all types that inherit from UnityEngine.Object
            var unityObjectTypes = GetAllUnityObjectTypes();

            // Group by category
            var categories = new Dictionary<string, List<Type>>();

            foreach (var type in unityObjectTypes)
            {
                var category = GetTypeCategory(type);
                if (!categories.ContainsKey(category))
                    categories[category] = new List<Type>();
                categories[category].Add(type);
            }

            // Add menu items for each category
            foreach (var category in categories.Keys.OrderBy(k => k))
            {
                foreach (var type in categories[category].OrderBy(t => t.Name))
                {
                    var menuPath = $"{category}/{type.Name}";
                    var isSelected = catalogKey.AssociatedType == type;

                    menu.AddItem(new GUIContent(menuPath), isSelected, () => {
                        Undo.RecordObject(catalogKey, "Change Catalog Key Type");
                        catalogKey.SetAssociatedType(type);
                        EditorUtility.SetDirty(catalogKey);
                        UpdateUI();
                    });
                }
            }
            
            menu.ShowAsContext();
        }
        
        private Type[] GetAllUnityObjectTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => 
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch
                    {
                        return new Type[0];
                    }
                })
                .Where(type => 
                    type != null && 
                    !type.IsAbstract && 
                    !type.IsGenericTypeDefinition &&
                    typeof(UnityEngine.Object).IsAssignableFrom(type))
                .ToArray();
        }
        
        private string GetTypeCategory(Type type)
        {
            // Basic Unity Object
            if (type == typeof(UnityEngine.Object))
                return "Core";
                
            // Components and MonoBehaviours
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
                return "MonoBehaviour";
            if (typeof(Component).IsAssignableFrom(type))
                return "Component";
                
            // GameObjects
            if (type == typeof(GameObject))
                return "Core";
                
            // Assets
            if (typeof(ScriptableObject).IsAssignableFrom(type))
                return "ScriptableObject";
            if (typeof(Texture).IsAssignableFrom(type))
                return "Texture";
            if (typeof(Material).IsAssignableFrom(type))
                return "Material";
            if (typeof(Mesh).IsAssignableFrom(type))
                return "Mesh";
            if (typeof(AudioClip).IsAssignableFrom(type))
                return "Audio";
            if (typeof(Sprite).IsAssignableFrom(type))
                return "Sprite";
                
            // Determine by namespace
            if (type.Namespace != null)
            {
                if (type.Namespace.StartsWith("UnityEngine.UI"))
                    return "UI";
                if (type.Namespace.StartsWith("UnityEngine.Rendering"))
                    return "Rendering";
                if (type.Namespace.StartsWith("UnityEngine.Audio"))
                    return "Audio";
                if (type.Namespace.StartsWith("UnityEngine.Video"))
                    return "Video";
            }
            
            return "Other";
        }
    }
}
#endif
