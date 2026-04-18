using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridBuilder))]
public class GridBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);

        if (GUILayout.Button("Rebuild Grids"))
        {
            GridBuilder builder = (GridBuilder)target;
            Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Rebuild Grids");
            builder.RebuildGrids();
        }
    }
}
