using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshRenderer))]
public class WaterTile : MonoBehaviour
{
    [SerializeField] private int GridSize = 100;
    [SerializeField] private float CellSize = 0.2f;
    [SerializeField] private MeshFilter MeshFilter;
    [SerializeField] private MeshRenderer WaterRenderer;
    [SerializeField] private string MeshSavePath = "Assets/Generated/WaterTileMesh.asset";

    [Header("Foam Bake")]
    [SerializeField] private LayerMask BakeLayer = ~0;
    [SerializeField] private float FadeDistance = 2f;
    [SerializeField] private int TextureSize = 256;
    [SerializeField] private float RaycastHeight = 50f;
    [SerializeField] private float MaxDepth = 1f;

    private static readonly int FoamMapId     = Shader.PropertyToID("_FoamMap");
    private static readonly int FoamMapParams = Shader.PropertyToID("_FoamMapParams");

    private RenderTexture _foamRT;

    private void Start()
    {
         // BakeFoam();
    }
    private void OnDestroy()
    {
        if (_foamRT != null) _foamRT.Release();
        _foamRT = null;
    }

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

    public void BakeFoam()
    {
        float tileSize = GridSize * CellSize;
        float waterY   = transform.position.y;
        float half     = tileSize * 0.5f;
        Vector3 origin = transform.position - new Vector3(half, 0f, half);

        int   n              = TextureSize * TextureSize;
        float texelWorldSize = tileSize / TextureSize;

        int[] dx = { -1, 1,  0, 0 };
        int[] dz = {  0, 0, -1, 1 };

        bool   hitAnything  = false;
        bool[] isCollision  = new bool[n];

        for (int z = 0; z < TextureSize; z++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float wx  = origin.x + (x + 0.5f) * texelWorldSize;
                float wz  = origin.z + (z + 0.5f) * texelWorldSize;
                var   ray = new Ray(new Vector3(wx, waterY + RaycastHeight, wz), Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, RaycastHeight + MaxDepth, BakeLayer) && hit.point.y >= waterY)
                {
                    hitAnything = true;
                    isCollision[z * TextureSize + x] = true;
                }
            }
        }

        if (!hitAnything)
        {
            var emptyBlock = new MaterialPropertyBlock();
            WaterRenderer.GetPropertyBlock(emptyBlock);
            emptyBlock.SetTexture(FoamMapId, Texture2D.blackTexture);
            emptyBlock.SetVector(FoamMapParams, new Vector4(origin.x, origin.z, tileSize, waterY));
            WaterRenderer.SetPropertyBlock(emptyBlock);
            return;
        }

        // BFS distance transform
        int[] dist = new int[n];
        for (int i = 0; i < n; i++) dist[i] = int.MaxValue;

        var queue = new Queue<int>();
        for (int i = 0; i < n; i++)
            if (isCollision[i]) { dist[i] = 0; queue.Enqueue(i); }

        while (queue.Count > 0)
        {
            int idx = queue.Dequeue();
            int cx  = idx % TextureSize;
            int cz  = idx / TextureSize;
            for (int d = 0; d < 4; d++)
            {
                int nx   = cx + dx[d];
                int nz   = cz + dz[d];
                if (nx < 0 || nx >= TextureSize || nz < 0 || nz >= TextureSize) continue;
                int nIdx = nz * TextureSize + nx;
                if (dist[nIdx] > dist[idx] + 1)
                {
                    dist[nIdx] = dist[idx] + 1;
                    queue.Enqueue(nIdx);
                }
            }
        }

        // Build foam values and upload to GPU
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
            data[i] = isCollision[i] ? 0f : Mathf.Clamp01(1f - dist[i] * texelWorldSize / FadeDistance);

        var tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RFloat, false);
        tex.SetPixelData(data, 0);
        tex.Apply();

        if (_foamRT != null) _foamRT.Release();
        _foamRT = new RenderTexture(new RenderTextureDescriptor(TextureSize, TextureSize, RenderTextureFormat.RFloat, 0));
        Graphics.Blit(tex, _foamRT);
        Destroy(tex);

        var block = new MaterialPropertyBlock();
        WaterRenderer.GetPropertyBlock(block);
        block.SetTexture(FoamMapId,    _foamRT);
        block.SetVector(FoamMapParams, new Vector4(origin.x, origin.z, tileSize, waterY));
        WaterRenderer.SetPropertyBlock(block);
    }
}
