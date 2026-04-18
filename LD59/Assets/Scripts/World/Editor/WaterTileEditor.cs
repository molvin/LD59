using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaterTile))]
public class WaterTileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);

        if (GUILayout.Button("Build Grid Mesh"))
        {
            WaterTile tile = (WaterTile)target;
            Undo.RegisterFullObjectHierarchyUndo(tile.gameObject, "Build Grid Mesh");
            tile.BuildGridMesh();
        }
    }
}
