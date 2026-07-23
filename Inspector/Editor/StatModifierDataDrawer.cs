using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatSystem.Editor
{
    [CustomPropertyDrawer(typeof(ModifierData<>))]
    public class StatModifierDataDrawer : PropertyDrawer
    {
        // Stable object context tracking using SerializedObject hashcodes
        private static readonly Dictionary<int, List<VisualElement>> _activeElementsPerContext = new();

        public override VisualElement CreatePropertyGUI (SerializedProperty property)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.SpaceBetween;
            container.style.marginBottom = 2;

            // Initialize a stable 2px left border on the main container to prevent any layout shifting
            container.style.borderLeftWidth = 2;
            container.style.borderLeftColor = Color.clear;
            
            var statProp = property.FindPropertyRelative("stat");
            var typeProp = property.FindPropertyRelative("type");
            var valueProp = property.FindPropertyRelative("value");

            var lockAttr = fieldInfo.GetCustomAttribute<LockModifierTypeAttribute>();
            var enumAttr = fieldInfo.GetCustomAttribute<StatEnumAttribute>();

            if (lockAttr != null)
            {
                typeProp.enumValueIndex = (int)lockAttr.ForcedType;
                property.serializedObject.ApplyModifiedProperties();
            }

            int contextId = property.serializedObject.GetHashCode();

            // 1. STAT FIELD - Adaptive scaling layout
            VisualElement statField;
            if (enumAttr != null)
            {
                var names = Enum.GetNames(enumAttr.EnumType);
                int currentIndex = Mathf.Max(0, Array.IndexOf(names, statProp.stringValue));

                var enumField = new PopupField<string>(new List<string>(names), currentIndex);
                enumField.name = "StatField_Popup";
                enumField.RegisterValueChangedCallback(evt =>
                {
                    statProp.stringValue = evt.newValue;
                    property.serializedObject.ApplyModifiedProperties();
                    UpdateDuplicateHighlighting(contextId, property.serializedObject);
                });
                statField = enumField;

                if (string.IsNullOrEmpty(statProp.stringValue) && names.Length > 0)
                {
                    statProp.stringValue = names[0];
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                var textField = new TextField();
                textField.name = "StatField_Text";
                textField.BindProperty(statProp);
                textField.RegisterValueChangedCallback(_ => UpdateDuplicateHighlighting(contextId, property.serializedObject));
                statField = textField;
            }

            statField.style.flexGrow = 1f;
            statField.style.flexShrink = 1f;
            statField.style.flexBasis = StyleKeyword.Null;
            statField.style.marginRight = 6;

            // 2. TYPE FIELD - Fixed explicit styling with hidden label
            var typeField = new PropertyField(typeProp);
            typeField.name = "TypeField_Property";
            typeField.style.width = 120;
            typeField.style.flexGrow = 0f;
            typeField.style.flexShrink = 0f;
            typeField.style.marginRight = 6;
            if (lockAttr != null) typeField.SetEnabled(false);

            typeField.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var label = typeField.Q<Label>();
                if (label != null) 
                    label.style.display = DisplayStyle.None;
            });
            typeField.RegisterCallback<SerializedPropertyChangeEvent>(_ => UpdateDuplicateHighlighting(contextId, property.serializedObject));

            // 3. VALUE FIELD - Fixed explicit styling with hidden label
            var valueField = new PropertyField(valueProp);
            valueField.style.width = 80;
            valueField.style.flexGrow = 0f;
            valueField.style.flexShrink = 0f;
            valueField.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var label = valueField.Q<Label>();
                if (label != null) 
                    label.style.display = DisplayStyle.None;
            });

            container.Add(statField);
            container.Add(typeField);
            container.Add(valueField);

            container.userData = property.propertyPath;

            // Lifecycle listeners
            container.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                if (_activeElementsPerContext.ContainsKey(contextId) == false)
                    _activeElementsPerContext[contextId] = new List<VisualElement>();
                
                _activeElementsPerContext[contextId].RemoveAll(el => el.panel == null);

                if (_activeElementsPerContext[contextId].Contains(container) == false)
                    _activeElementsPerContext[contextId].Add(container);
                UpdateDuplicateHighlighting(contextId, property.serializedObject);
            });

            container.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                if (_activeElementsPerContext.TryGetValue(contextId, out var list))
                {
                    list.Remove(container);
                    if (list.Count == 0) _activeElementsPerContext.Remove(contextId);
                }
                UpdateDuplicateHighlighting(contextId, property.serializedObject);
            });

            return container;
        }

        private void UpdateDuplicateHighlighting (int contextId, SerializedObject serializedObject)
        {
            if (_activeElementsPerContext.TryGetValue(contextId, out var list) == false) 
                return;

            list.RemoveAll(el => el.panel == null);

            try
            {
                if (serializedObject == null) 
                    return;
                serializedObject.Update();
                if (serializedObject.targetObject == null) 
                    return;
            }
            catch
            {
                return;
            }

            var counts = new Dictionary<string, int>();
            var elementDataList = new List<(string compositeKey, VisualElement container)>();

            foreach (var container in list)
            {
                string path = container.userData as string ?? string.Empty;
                string arrayRootPath = path.Contains("[") ? path.Substring(0, path.LastIndexOf('[')) : path;

                VisualElement statField = container.Q<PopupField<string>>("StatField_Popup") ??
                                         (VisualElement)container.Q<TextField>("StatField_Text");

                string statVal = string.Empty;
                if (statField is PopupField<string> popup) 
                    statVal = popup.value;
                else if (statField is TextField text) 
                    statVal = text.value;

                var dataProp = serializedObject.FindProperty(path);
                if (dataProp == null) 
                    continue;

                var typeProp = dataProp.FindPropertyRelative("type");
                int typeVal = typeProp != null ? typeProp.enumValueIndex : -1;

                if (typeVal == -1 || string.IsNullOrEmpty(statVal) || statField == null) 
                    continue;

                string compositeKey = $"{arrayRootPath}_{statVal}_{typeVal}";

                if (counts.ContainsKey(compositeKey) == false) 
                    counts[compositeKey] = 0;
                counts[compositeKey]++;

                elementDataList.Add((compositeKey, container));
            }

            // High-fidelity soft yellow-orange warning color
            var warningColor = new StyleColor(new Color(1f, 0.6f, 0f, 0.9f));

            foreach (var data in elementDataList)
            {
                string path = data.container.userData as string ?? string.Empty;
                bool isPartOfList = path.Contains("[");

                if (counts[data.compositeKey] > 1 && isPartOfList)
                {
                    // Paint the left border of the entire element row
                    data.container.style.borderLeftColor = warningColor;
                }
                else
                {
                    // Clean reset to transparent
                    data.container.style.borderLeftColor = Color.clear;
                }
            }
        }
    }
}
