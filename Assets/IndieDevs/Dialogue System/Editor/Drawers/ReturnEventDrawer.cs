using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem
{
    [CustomPropertyDrawer(typeof(ReturnEvent<>), true)]
    public class ReturnEventDrawer : PropertyDrawer
    {
        private const int numLines = 3;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = 2f;

            Rect gameObjectRect = new Rect(position.x, position.y, position.width, lineHeight);
            Rect componentRect = new Rect(position.x, position.y + lineHeight + padding, position.width, lineHeight);
            Rect fieldRect = new Rect(position.x, position.y + 2 * (lineHeight + padding), position.width, lineHeight);

            SerializedProperty targetGameObjectProp = property.FindPropertyRelative("targetGameObject");
            SerializedProperty componentNameProp = property.FindPropertyRelative("componentName");
            SerializedProperty fieldOrPropertyNameProp = property.FindPropertyRelative("fieldOrPropertyName");

            Type requiredType = GetGenericArgumentType(fieldInfo.FieldType);

            GUIContent gameObjectLabel = new GUIContent("Game Object");
            EditorGUI.PropertyField(gameObjectRect, targetGameObjectProp, gameObjectLabel);

            if (targetGameObjectProp.objectReferenceValue != null)
            {
                GameObject targetGameObject = (GameObject)targetGameObjectProp.objectReferenceValue;

                Component[] components = targetGameObject.GetComponents<Component>();
                List<string> componentNames = new List<string>();
                Dictionary<string, FieldInfo> classFields = new Dictionary<string, FieldInfo>();

                foreach (Component component in components)
                {
                    componentNames.Add(component.GetType().Name);

                    FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        if (IsClassOrStruct(field.FieldType))
                        {
                            componentNames.Add($"{component.GetType().Name}.{field.Name}");
                            classFields.Add($"{component.GetType().Name}.{field.Name}", field);
                        }
                    }
                }

                int selectedComponentIndex = componentNames.IndexOf(componentNameProp.stringValue);
                selectedComponentIndex = EditorGUI.Popup(componentRect, "Component", selectedComponentIndex, componentNames.ToArray());
                if (selectedComponentIndex >= 0)
                {
                    componentNameProp.stringValue = componentNames[selectedComponentIndex];
                }

                if (selectedComponentIndex >= 0)
                {
                    string selectedComponentName = componentNames[selectedComponentIndex];
                    List<string> fieldAndPropertyNames = new List<string>();

                    if (classFields.ContainsKey(selectedComponentName))
                    {
                        FieldInfo nestedClassField = classFields[selectedComponentName];
                        GatherFields(nestedClassField.FieldType, requiredType, fieldAndPropertyNames);
                    }
                    else
                    {
                        Component selectedComponent = components[selectedComponentIndex];
                        GatherFieldsAndProperties(selectedComponent.GetType(), requiredType, fieldAndPropertyNames);
                    }

                    int selectedFieldIndex = fieldAndPropertyNames.IndexOf(fieldOrPropertyNameProp.stringValue);
                    selectedFieldIndex = EditorGUI.Popup(fieldRect, "Field/Property", selectedFieldIndex, fieldAndPropertyNames.ToArray());

                    if (selectedFieldIndex >= 0)
                    {
                        fieldOrPropertyNameProp.stringValue = fieldAndPropertyNames[selectedFieldIndex];
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        private Type GetGenericArgumentType(Type returnType)
        {
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ReturnEvent<>))
            {
                return returnType.GetGenericArguments()[0];
            }
            return null;
        }

        private void GatherFieldsAndProperties(Type type, Type requiredType, List<string> result)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (IsValidType(field.FieldType, requiredType))
                {
                    result.Add(field.Name);
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (IsValidType(property.PropertyType, requiredType))
                {
                    result.Add(property.Name);
                }
            }
        }

        private void GatherFields(Type classType, Type requiredType, List<string> result)
        {
            FieldInfo[] fields = classType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (IsValidType(field.FieldType, requiredType))
                {
                    result.Add(field.Name);
                }
            }
        }

        private bool IsValidType(Type fieldType, Type requiredType)
        {
            return fieldType == requiredType || requiredType.IsAssignableFrom(fieldType);
        }

        private bool IsClassOrStruct(Type type)
        {
            return (type.IsClass || (type.IsValueType && !type.IsPrimitive))
            && type != typeof(int) && type != typeof(float) && type != typeof(string) && type != typeof(bool);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight + 2f) * numLines;
        }
    }
}
