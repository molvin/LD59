using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(GridBuilder))]
public class WaterFoamBaker : MonoBehaviour
{
    [SerializeField] private LayerMask BakeLayer = ~0;
    [SerializeField] private float FadeDistance = 2f;
    [SerializeField] private int TextureSize = 256;
    [SerializeField] private float RaycastHeight = 50f;
    [SerializeField] private float MaxDepth = 100f;
    [SerializeField] private bool DebugOutput = false;
    [SerializeField] private string OutputPath = "Assets/Generated/FoamMap";

    private static readonly int FoamMapId     = Shader.PropertyToID("_FoamMap");
    private static readonly int FoamMapParams = Shader.PropertyToID("_FoamMapParams");

    private readonly List<RenderTexture> _tileRTs = new();

    private void Start() => Bake();

    private void OnDestroy()
    {
        foreach (var rt in _tileRTs) rt?.Release();
        _tileRTs.Clear();
    }

    public void Bake()
    {
        var builder  = GetComponent<GridBuilder>();
        float tileSize      = builder.TileWorldSize;
        Transform tilesRoot = builder.TilesRoot;
        float waterY        = tilesRoot.position.y;

        foreach (var rt in _tileRTs) rt?.Release();
        _tileRTs.Clear();

        int   n              = TextureSize * TextureSize;
        float texelWorldSize = tileSize / TextureSize;

        int[] dx = new [] {-1,  1,  0,  0};
        int[] dz = new []{ 0,  0, -1,  1};

        for (int ti = 0; ti < tilesRoot.childCount; ti++)
        {
            var tile     = tilesRoot.GetChild(ti);
            var renderer = tile.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            Vector3 origin = tile.position;

            // Raycast pass
            bool hitAnything = false;
            bool[] isCollision = new bool[n];
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
                renderer.GetPropertyBlock(emptyBlock);
                emptyBlock.SetTexture(FoamMapId, Texture2D.blackTexture);
                emptyBlock.SetVector(FoamMapParams, new Vector4(origin.x, origin.z, tileSize, waterY));
                renderer.SetPropertyBlock(emptyBlock);
                continue;
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

            // Build foam values
            float[] data = new float[n];
            for (int i = 0; i < n; i++)
                data[i] = isCollision[i] ? 0f : Mathf.Clamp01(1f - dist[i] * texelWorldSize / FadeDistance);

            // Upload to GPU
            var tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RFloat, false);
            tex.SetPixelData(data, 0);
            tex.Apply();

            var desc = new RenderTextureDescriptor(TextureSize, TextureSize, RenderTextureFormat.RFloat, 0);
            var rt   = new RenderTexture(desc);
            Graphics.Blit(tex, rt);
            DestroyImmediate(tex);
            _tileRTs.Add(rt);

            // Set per-tile via MaterialPropertyBlock (avoids creating material instances)
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetTexture(FoamMapId,    rt);
            block.SetVector(FoamMapParams, new Vector4(origin.x, origin.z, tileSize, waterY));
            renderer.SetPropertyBlock(block);

            if (!string.IsNullOrEmpty(OutputPath) && DebugOutput)
            {
                ExportToPng(rt, ti);
            }
        }

        Debug.Log($"WaterFoamBaker: baked {tilesRoot.childCount} tiles at {TextureSize}×{TextureSize}.");
    }

    private void ExportToPng(RenderTexture rt, int index)
    {
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var readback = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false);
        readback.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readback.Apply();
        RenderTexture.active = prev;

        var export = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        var src    = readback.GetPixelData<float>(0);
        var dst    = new Color32[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            byte v = (byte)(Mathf.Clamp01(src[i]) * 255f);
            dst[i] = new Color32(v, v, v, 255);
        }
        export.SetPixels32(dst);
        export.Apply();

        string path = $"{OutputPath}_{index}.png";
        string dir  = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(path, export.EncodeToPNG());

        DestroyImmediate(readback);
        DestroyImmediate(export);
        Debug.Log($"WaterFoamBaker: saved tile {index} to {path}");
    }
}
