using UnityEditor;
using UnityEngine;

public class SXL_GrindGeneratorWindow : EditorWindow
{
    [MenuItem("SXL/Grind Surface Generator (Experimental)")]
    private static void Init()
    {
        var window = GetWindow<SXL_GrindGeneratorWindow>();
        window.titleContent = new GUIContent("Grind Surface Generator");
        window.Show();
    }

    private void OnEnable()
    {
        containerStyle = new GUIStyle() {padding = new RectOffset(10, 10, 10, 10)};
    }

    private GUIStyle containerStyle;
    private static GameObject scaleRefInstance;

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(containerStyle);
        {
            EditorGUI.BeginChangeCheck();

            if (EditorGUI.EndChangeCheck())
            {
            }
        }
        EditorGUILayout.EndVertical();
    }
}
