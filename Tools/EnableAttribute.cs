using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(EnableAttribute))]
public class DisableIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Pega o atributo IsEnableAttribute
        EnableAttribute isEnableAttribute = (EnableAttribute)attribute;

        // Pega o nome da propriedade de refer�ncia, se existir
        if (!string.IsNullOrEmpty(isEnableAttribute.ReferenceProperty))
        {
            // Pega a propriedade de refer�ncia do SerializedObject
            SerializedProperty referenceProperty = property.serializedObject.FindProperty(isEnableAttribute.ReferenceProperty);

            if (referenceProperty != null)
            {
                // Define o estado do campo com base no valor booleano da propriedade de refer�ncia
                GUI.enabled = referenceProperty.boolValue == isEnableAttribute.DisabledState;
            }
            else
            {
                Debug.LogWarning($"Property '{isEnableAttribute.ReferenceProperty}' not founded on object.");
            }
        }
        else
        {
            // Se n�o houver uma propriedade de refer�ncia, usa o estado DisabledState diretamente
            GUI.enabled = isEnableAttribute.DisabledState;
        }

        // Desenha a propriedade
        EditorGUI.PropertyField(position, property, label);

        // Restaura o estado original da GUI
        GUI.enabled = true;
    }
}
#endif

/// <summary> 
/// Atributo para desativar o campo com base em uma propriedade de refer�ncia ou em um estado fixo.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class EnableAttribute : PropertyAttribute
{
    public bool DisabledState;
    public string ReferenceProperty;

    // Construtor para estado fixo
    public EnableAttribute(bool state)
    {
        DisabledState = state;
    }

    // Construtor para estado condicional com uma propriedade de refer�ncia
    public EnableAttribute(string referenceProperty, bool invertState = false)
    {
        ReferenceProperty = referenceProperty;
        DisabledState = !invertState;
    }
}
