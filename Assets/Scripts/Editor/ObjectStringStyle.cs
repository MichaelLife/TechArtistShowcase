using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectStringStyle : ScriptableObject
{
    public List<StringStyle> styles = new List<StringStyle>();
}

[System.Serializable]
public class StringStyle
{
    public string nombre;
    public Font font;
    public Color fontColor;
    public Color backgroundColor;
    public FontStyle fontStyle;
    public TextAnchor alignment;
    public Texture2D Icon;
    public int size;
}
