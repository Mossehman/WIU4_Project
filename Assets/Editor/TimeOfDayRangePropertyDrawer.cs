using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(MinMaxEnum<>), true)]
public class MinMaxEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty startProp = property.FindPropertyRelative("start");
        SerializedProperty endProp = property.FindPropertyRelative("end");

        if (startProp == null || endProp == null)
        {
            EditorGUI.LabelField(position, "Invalid MinMaxEnum Usage");
            return;
        }

        Type enumType = fieldInfo.FieldType.GetGenericArguments()[0];
        Array enumValues = Enum.GetValues(enumType);
        int enumLength = enumValues.Length - 1; // Exclude last value

        // Convert to array and remove last enum entry
        string[] enumNames = Enum.GetNames(enumType).Take(enumLength).ToArray();
        int[] validIndices = Enumerable.Range(0, enumLength).ToArray();

        // Draw the label
        position = EditorGUI.PrefixLabel(position, label);

        // Calculate field widths
        float fieldWidth = position.width / 2 - 5;
        Rect startRect = new Rect(position.x, position.y, fieldWidth, position.height);
        Rect endRect = new Rect(position.x + fieldWidth + 10, position.y, fieldWidth, position.height);

        // Draw dropdowns excluding last enum
        startProp.enumValueIndex = EditorGUI.IntPopup(startRect, startProp.enumValueIndex, enumNames, validIndices);
        endProp.enumValueIndex = EditorGUI.IntPopup(endRect, endProp.enumValueIndex, enumNames, validIndices);
    }
}
