using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Jungle.Catalog.Editor
{
    [CustomPropertyDrawer(typeof(Catalog))]
    public class CatalogPropertyDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, float> elementHeightCache = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            var entriesProperty = property.FindPropertyRelative("entries");
            var contentRect = EditorGUI.IndentedRect(new Rect(position.x, foldoutRect.yMax + EditorGUIUtility.standardVerticalSpacing, position.width, position.height - EditorGUIUtility.singleLineHeight));

            var y = contentRect.y;
            var boxPadding = 4f;

            for (var i = 0; i < entriesProperty.arraySize; i++)
            {
                var entryProperty = entriesProperty.GetArrayElementAtIndex(i);
                var propertyHeight = EditorGUI.GetPropertyHeight(entryProperty, GUIContent.none, true);
                elementHeightCache[entryProperty.propertyPath] = propertyHeight;

                var boxHeight = propertyHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2f + boxPadding * 2f;
                var boxRect = new Rect(contentRect.x, y, contentRect.width, boxHeight);
                GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

                var labelRect = new Rect(boxRect.x + boxPadding, boxRect.y + boxPadding, boxRect.width - boxPadding * 2f - 20f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, GetEntryLabel(entryProperty), EditorStyles.boldLabel);

                var removeRect = new Rect(boxRect.xMax - boxPadding - 18f, boxRect.y + boxPadding, 18f, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(removeRect, EditorGUIUtility.IconContent("Toolbar Minus"), EditorStyles.miniButton))
                {
                    RemoveEntry(property.serializedObject, entriesProperty.propertyPath, i);
                    EditorGUI.EndProperty();
                    return;
                }

                var fieldRect = new Rect(boxRect.x + boxPadding, labelRect.yMax + EditorGUIUtility.standardVerticalSpacing, boxRect.width - boxPadding * 2f, propertyHeight);
                EditorGUI.PropertyField(fieldRect, entryProperty, GUIContent.none, true);

                y += boxHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            var buttonRect = new Rect(contentRect.x, y, 24f, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyles.miniButton))
            {
                ShowAddEntryMenu(property.serializedObject, entriesProperty.propertyPath);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
            {
                return height;
            }

            height += EditorGUIUtility.standardVerticalSpacing;

            var entriesProperty = property.FindPropertyRelative("entries");
            for (var i = 0; i < entriesProperty.arraySize; i++)
            {
                var entryProperty = entriesProperty.GetArrayElementAtIndex(i);
                if (!elementHeightCache.TryGetValue(entryProperty.propertyPath, out var propertyHeight))
                {
                    propertyHeight = EditorGUI.GetPropertyHeight(entryProperty, GUIContent.none, true);
                    elementHeightCache[entryProperty.propertyPath] = propertyHeight;
                }

                var boxHeight = propertyHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2f + 8f;
                height += boxHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            height += EditorGUIUtility.singleLineHeight;
            return height;
        }

        private void ShowAddEntryMenu(SerializedObject serializedObject, string entriesPropertyPath)
        {
            var menu = new GenericMenu();
            var entryTypes = TypeCache.GetTypesDerivedFrom<CatalogEntry>()
                .Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericType && type.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(type => type.FullName)
                .ToList();

            if (entryTypes.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No catalog entry types found"));
            }
            else
            {
                foreach (var entryType in entryTypes)
                {
                    var content = new GUIContent(BuildMenuLabel(entryType));
                    menu.AddItem(content, false, () => AddEntry(serializedObject, entriesPropertyPath, entryType));
                }
            }

            menu.ShowAsContext();
        }

        private static string BuildMenuLabel(Type type)
        {
            if (string.IsNullOrEmpty(type.Namespace))
            {
                return type.Name;
            }

            return $"{type.Namespace.Replace('.', '/')}/{type.Name}";
        }

        private void AddEntry(SerializedObject serializedObject, string entriesPropertyPath, Type entryType)
        {
            Undo.RecordObjects(serializedObject.targetObjects, "Add Catalog Entry");
            serializedObject.Update();

            var entriesProperty = serializedObject.FindProperty(entriesPropertyPath);
            var newIndex = entriesProperty.arraySize;
            entriesProperty.arraySize++;

            var newElement = entriesProperty.GetArrayElementAtIndex(newIndex);
            newElement.managedReferenceValue = Activator.CreateInstance(entryType);

            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveEntry(SerializedObject serializedObject, string entriesPropertyPath, int index)
        {
            Undo.RecordObjects(serializedObject.targetObjects, "Remove Catalog Entry");
            serializedObject.Update();

            var entriesProperty = serializedObject.FindProperty(entriesPropertyPath);
            if (index >= entriesProperty.arraySize)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            entriesProperty.DeleteArrayElementAtIndex(index);
            if (index < entriesProperty.arraySize)
            {
                entriesProperty.DeleteArrayElementAtIndex(index);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static string GetEntryLabel(SerializedProperty entryProperty)
        {
            var fullTypeName = entryProperty.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(fullTypeName))
            {
                return "Unassigned Entry";
            }

            var spaceIndex = fullTypeName.IndexOf(' ');
            if (spaceIndex >= 0 && spaceIndex + 1 < fullTypeName.Length)
            {
                fullTypeName = fullTypeName[(spaceIndex + 1)..];
            }

            var lastDot = fullTypeName.LastIndexOf('.');
            if (lastDot >= 0 && lastDot + 1 < fullTypeName.Length)
            {
                fullTypeName = fullTypeName[(lastDot + 1)..];
            }

            return fullTypeName;
        }
    }
}
