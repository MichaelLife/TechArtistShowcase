using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;

/// <summary> Sets a background color for game objects in the Hierarchy tab</summary>
[UnityEditor.InitializeOnLoad]
#endif
public class HierarchyObjectColor
{
    static string[] dataArray;
    static string path;
    private static ObjectStringStyle objectStringStyle;

    private static Vector2 offset = new Vector2(20, 1);

    static HierarchyObjectColor()
    {
        dataArray = AssetDatabase.FindAssets("t:ObjectStringStyle");

        if (dataArray != null)
        {
            path = AssetDatabase.GUIDToAssetPath(dataArray[0]);

            objectStringStyle = AssetDatabase.LoadAssetAtPath<ObjectStringStyle>(path);
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj != null)
        {
            Color backgroundColor = Color.white;
            Color textColor = Color.white;
            Color disabledTextColor = new Color(0.6f, 0.6f, 0.6f, 0.75f); 
            Color disabledBackgroundColor = new Color(0.3f, 0.3f, 0.3f); 
            Texture2D texture = null;
            FontStyle _f = FontStyle.Bold;
            TextAnchor _ta = TextAnchor.MiddleLeft;
            int _size = 12;
            Font _font = default;

            for (int i = 0; i < objectStringStyle.styles.Count; i++)
            {
                if(obj.name == objectStringStyle.styles[i].nombre)
                {
                    backgroundColor = objectStringStyle.styles[i].backgroundColor;
                    textColor = objectStringStyle.styles[i].fontColor;
                    _f = objectStringStyle.styles[i].fontStyle;
                    _ta = objectStringStyle.styles[i].alignment;
                    texture = objectStringStyle.styles[i].Icon;
                    _size = objectStringStyle.styles[i].size;
                    _font = objectStringStyle.styles[i].font;
                }
            }

            /*
            // Or you can use switch case
            switch (obj.name)
            {
                case "Suelo":
                    backgroundColor = Color.gray;
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "PLAYER":
                    backgroundColor = Color.cyan;
                    textColor = new Color(0f, 0f, 0f);
                    break;
                case "OverlayCanvas":
                    backgroundColor = new Color(1f, 0.2f, 0f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "SETTINGS":
                    backgroundColor = Color.black;
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "SPAWN MANAGER":
                    backgroundColor = Color.green;
                    textColor = new Color(0f, 0f, 0f);
                    break;
                case "CameraCanvas":
                    backgroundColor = new Color(0.1f, 0.4f, 0.9f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case "CheatCanvas":
                    backgroundColor = new Color(0.2f, 0.2f, 0.3f);
                    textColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
            }
            */

            if (backgroundColor != Color.white)
            {
                Rect offsetRect = new Rect(selectionRect.position + offset, selectionRect.size);
                Rect bgRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width + 50, selectionRect.height);

                string name = obj.name;

                if (EditorUtility.GetObjectEnabled(obj) == 0)
                {
                    Rect bgRect2 = new Rect(selectionRect.x, selectionRect.y, selectionRect.width + 50, selectionRect.height);
                    EditorGUI.DrawRect(bgRect2, new Color(0.278431f, 0.278431f, 0.278431f));

                    textColor.a = 0.35f;
                    backgroundColor.a = 0.35f;

                    _f = FontStyle.Italic;
                }

                EditorGUI.DrawRect(bgRect, backgroundColor);

                EditorGUI.LabelField(offsetRect, name, new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = textColor },
                    fontStyle = _f,
                    alignment = _ta,
                    fontSize = _size,
                    font = _font
                }
                );

                if (texture != null)
                    EditorGUI.DrawPreviewTexture(new Rect(selectionRect.position, new Vector2(selectionRect.height, selectionRect.height)), texture);
            }
        }
    }
}