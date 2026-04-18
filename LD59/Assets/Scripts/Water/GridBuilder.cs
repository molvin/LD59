using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    [SerializeField] private Transform Root;
    [SerializeField] private int Tiles = 5;
    [SerializeField] private int GridSize = 100;
    [SerializeField] private float CellSize = 0.2f;
    [SerializeField] private Material TileMaterial;

    public float      TileWorldSize => GridSize * CellSize;
    public Transform  TilesRoot    => Root;

    public (Vector3 worldOrigin, float totalSize) GetWaterBounds()
    {
        return (Root.position, Tiles * GridSize * CellSize);
    }

    public void RebuildGrids()
    {
        for (int i = Root.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(Root.GetChild(i).gameObject);
        }

        float tileWorldSize = GridSize * CellSize;

        for (int tx = 0; tx < Tiles; tx++)
        {
            for (int tz = 0; tz < Tiles; tz++)
            {
                GameObject tile = new($"Tile_{tx}_{tz}");
                tile.transform.SetParent(Root);
                tile.transform.localPosition = new Vector3(tx * tileWorldSize, 0f, tz * tileWorldSize);

                MeshFilter mf = tile.AddComponent<MeshFilter>();
                MeshRenderer mr = tile.AddComponent<MeshRenderer>();
                mf.sharedMesh = BuildTileMesh();
                mr.sharedMaterial = TileMaterial;
            }
        }
    }

    private Mesh BuildTileMesh()
    {
        int vertexCount = GridSize + 1;
        Vector3[] vertices = new Vector3[vertexCount * vertexCount];
        Vector2[] uvs = new Vector2[vertexCount * vertexCount];
        int[] triangles = new int[GridSize * GridSize * 6];

        for (int z = 0; z < vertexCount; z++)
        {
            for (int x = 0; x < vertexCount; x++)
            {
                int i = z * vertexCount + x;
                vertices[i] = new Vector3(x * CellSize, 0f, z * CellSize);
                uvs[i] = new Vector2((float)x / GridSize, (float)z / GridSize);
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

        Mesh mesh = new();
        if (vertexCount * vertexCount > 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}
