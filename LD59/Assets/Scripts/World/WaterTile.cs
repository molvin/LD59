using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WaterTile : MonoBehaviour
{
    [SerializeField] private int GridSize = 100;
    [SerializeField] private float CellSize = 0.2f;
    [SerializeField] private MeshFilter MeshFilter;
    [SerializeField] private string MeshSavePath = "Assets/Generated/WaterTileMesh.asset";

    public void BuildGridMesh()
    {
        int vertexCount = GridSize + 1;
        Vector3[] vertices  = new Vector3[vertexCount * vertexCount];
        Vector2[] uvs       = new Vector2[vertexCount * vertexCount];
        int[]     triangles = new int[GridSize * GridSize * 6];

        float half = GridSize * CellSize * 0.5f;
        for (int z = 0; z < vertexCount; z++)
        {
            for (int x = 0; x < vertexCount; x++)
            {
                int i = z * vertexCount + x;
                vertices[i] = new Vector3(x * CellSize - half, 0f, z * CellSize - half);
                uvs[i]      = new Vector2((float)x / GridSize, (float)z / GridSize);
            }
        }

        for (int z = 0, tri = 0; z < GridSize; z++)
        {
            for (int x = 0; x < GridSize; x++, tri += 6)
            {
                int i = z * vertexCount + x;
                triangles[tri + 0] = i;
                triangles[tri + 1] = i + vertexCount;
                triangles[tri + 2] = i + 1;
                triangles[tri + 3] = i + 1;
                triangles[tri + 4] = i + vertexCount;
                triangles[tri + 5] = i + vertexCount + 1;
            }
        }

        Mesh mesh = new() { name = "WaterTileMesh" };
        if (vertexCount * vertexCount > 65535)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices  = vertices;
        mesh.uv        = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

#if UNITY_EDITOR
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(MeshSavePath));
        AssetDatabase.CreateAsset(mesh, MeshSavePath);
        AssetDatabase.SaveAssets();
        MeshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshSavePath);
        EditorUtility.SetDirty(MeshFilter);
#endif
    }
}
