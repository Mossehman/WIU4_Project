using UnityEditor;


[CustomEditor(typeof(BaseNoise), true)]
public class NoiseEditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
    
        SerializedProperty useRandomSeedProp = serializedObject.FindProperty("useRandomSeed");
        SerializedProperty seedRangeProp = serializedObject.FindProperty("seedRange");

        DrawPropertiesExcluding(serializedObject, "seedRange");

        if (useRandomSeedProp.boolValue)
        {
            EditorGUILayout.PropertyField(seedRangeProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
