using System;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ButtonAttribute : PropertyAttribute
{
    public string Label { get; }
    public string Content { get; }
    public float Height { get; }

    public ButtonAttribute() : this("", "", 0) { }
    public ButtonAttribute(string content) : this("", content, 0) { }
    public ButtonAttribute(string label, string content) : this(label, content, 0) { }
    public ButtonAttribute(string label, string content, float height)
    {
        Label = label;
        Content = content;
        Height = height;
    }

    public ButtonAttribute(float height)
    {
        Label = "";
        Content = "";
        Height = height;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Boolean)
        {
            EditorGUI.LabelField(position, label.text, "Use [Button] with a bool field");
            return;
        }

        ButtonAttribute buttonAttr = (ButtonAttribute)attribute;
        string buttonLabel = string.IsNullOrEmpty(buttonAttr.Label) ? label.text : buttonAttr.Label;
        string buttonText = string.IsNullOrEmpty(buttonAttr.Content) ? label.text : buttonAttr.Content;
        float height = buttonAttr.Height > 0 ? buttonAttr.Height : EditorGUIUtility.singleLineHeight;

        Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, height);
        Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, height);

        if (string.IsNullOrEmpty(buttonAttr.Label))
        {
            buttonRect = position;
        }
        else
        {
            EditorGUI.LabelField(labelRect, buttonLabel);
        }

        if (GUI.Button(buttonRect, buttonText))
        {
            property.boolValue = !property.boolValue;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ButtonAttribute buttonAttr = (ButtonAttribute)attribute;
        return buttonAttr.Height > 0 ? buttonAttr.Height : EditorGUIUtility.singleLineHeight;
    }
}
#endif
