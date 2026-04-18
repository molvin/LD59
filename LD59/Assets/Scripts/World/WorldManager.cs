using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class WorldManager : Subsystem<WorldManager>
{
    const float VIEW_DISTANCE = 50;

    private Transform worldTransfrom;
    private Tile[] tileAssets;
    private Tile water;
    private Vector2 cellSize;
    private Vector2Int visibleCellDims;
    private List<GameObject> waterTiles = new(); 


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnGameStart()
    {
        WorldManager worldManager = Get();
        worldManager.LoadAssets();
    }

    private void LoadAssets()
    {
        tileAssets = Resources.LoadAll("Tiles", typeof(Tile)).Cast<Tile>().ToArray();
    }

    private void Start()
    {
        worldTransfrom = new GameObject("World").transform;
        SetupTiles();
    }

    private void SetupTiles()
    {
        foreach (Tile tile in tileAssets)
        {
            if (tile.Poolable)
            {
                water = tile;
                break;
            }
        }

        Assert.IsNotNull(water);
        Bounds tileBounds = Utils.GetBounds(water.gameObject);
        cellSize = new(tileBounds.size.x, tileBounds.size.z);

        visibleCellDims = new (Mathf.CeilToInt(VIEW_DISTANCE * 2 / cellSize.x), Mathf.CeilToInt(VIEW_DISTANCE * 2 / cellSize.y));

        float width = visibleCellDims.x * cellSize.x;
        float depth = visibleCellDims.y * cellSize.y;
        for (int y = 0; y < visibleCellDims.y; ++y)
        {
            for (int x = 0; x < visibleCellDims.x; ++x)
            {
                GameObject tile = Instantiate(water.gameObject, worldTransfrom);
                tile.transform.position = new(-width * 0.5f + x * cellSize.x, 0.0f, -depth * 0.5f + y * cellSize.y);
                waterTiles.Add(tile);
            }
        }
    }

    private void Update()
    {
        Vector3 playerPosition = GameManager.Get().Player.transform.position;

        float width = visibleCellDims.x * cellSize.x;
        float depth = visibleCellDims.y * cellSize.y;

        foreach (GameObject tile in waterTiles)
        {
            Vector3 relative = tile.transform.position - playerPosition;
            if (relative.x < -width * 0.5f)
            {
                tile.transform.position += new Vector3(width, 0, 0);
            }
            if (relative.x > width * 0.5f)
            {
                tile.transform.position -= new Vector3(width, 0, 0);
            }
            if (relative.z < -depth * 0.5f)
            {
                tile.transform.position += new Vector3(0, 0, depth);
            }
            if (relative.z > depth * 0.5f)
            {
                tile.transform.position -= new Vector3(0, 0, depth);
            }
        }
    }
}
