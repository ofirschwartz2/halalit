using UnityEditor;
using UnityEngine;

[System.Serializable]
public class MinMaxRange
{
    public float min;
    public float max;

    public MinMaxRange(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}

[CustomPropertyDrawer(typeof(MinMaxRange))]
public class MinMaxRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Calculate rect for fields and labels
        var minLabelRect = new Rect(position.x, position.y, 30, position.height);
        var minRect = new Rect(position.x + 35, position.y, 50, position.height);
        var maxLabelRect = new Rect(position.x + 90, position.y, 30, position.height);
        var maxRect = new Rect(position.x + 125, position.y, 50, position.height);

        // Draw fields and labels
        EditorGUI.LabelField(minLabelRect, new GUIContent("Min"));
        EditorGUI.PropertyField(minRect, property.FindPropertyRelative("min"), GUIContent.none);

        EditorGUI.LabelField(maxLabelRect, new GUIContent("Max"));
        EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("max"), GUIContent.none);

        EditorGUI.EndProperty();
    }
}