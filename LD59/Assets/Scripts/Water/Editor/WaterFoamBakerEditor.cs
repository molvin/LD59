using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaterFoamBaker))]
public class WaterFoamBakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);

        if (GUILayout.Button("Bake Foam Map"))
        {
            Undo.RegisterFullObjectHierarchyUndo(((WaterFoamBaker)target).gameObject, "Bake Foam Map");
            ((WaterFoamBaker)target).Bake();
        }
    }
}
