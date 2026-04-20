using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class WorldManager : Subsystem<WorldManager>
{
    const float VIEW_DISTANCE = 500;

    private Transform worldTransfrom;
    private Tile[] tileAssets;
    private Tile water;
    private Vector2 cellSize;
    private Vector2Int visibleCellDims;
    private List<GameObject> waterTiles = new(); 
    public List<GameObject> Destinations = new();

    private List<GameObject> seaguls = new();
    private List<Vector2> seagulVelocities = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnGameStart()
    {
        if (!SceneConfig.InitializePlayScene())
        {
            return;
        }

        WorldManager worldManager = Get();
        worldManager.LoadAssets();
    }

    private void LoadAssets()
    {
        tileAssets = Resources.LoadAll("Tiles", typeof(Tile)).Cast<Tile>().ToArray();
        worldTransfrom = new GameObject("World").transform;
        SetupTiles();

        GameObject seagulPrefab = Resources.Load("Seagul") as GameObject;
        for (int i = 0; i < 50; i++)
        {
            GameObject seagul = Instantiate(seagulPrefab, transform);
            Vector3 pos = Random.onUnitSphere * 100;
            pos.y = pos.y * 0.15f + 30;
            seagul.transform.position = pos;
            seaguls.Add(seagul);
            seagulVelocities.Add(Random.onUnitCircle * 5);
        }
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

    private void Start()
    {
        var dropOff = FindFirstObjectByType<Dropoff>();
        var config = FindFirstObjectByType<SceneConfig>();
        if (dropOff != null && config != null && config.SetupPlayScene && config.DebugSpawnIsland != null)
        {
            GameObject island = Instantiate(config.DebugSpawnIsland);
            Vector3 dir = Random.onUnitCircle * config.DebugSpawnDistance;
            dir.z = dir.y;
            dir.y = 0;
            island.transform.position = dir;

            Destinations.Add(island);
            dropOff.AddToDropOff(island);
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

        SeagulSim();
    }

    private void SeagulSim()
    {
        float vision = 50;
        float minDist = 20;
        float maxSpeed = 5;
        float cohesion = 0.05f;
        float align = 0.5f;
        float separation = 0.5f;
        float maxDistanceFromPlayer = 200;
        float turnStrength = 2.0f;

        for (int i = 0; i < seaguls.Count; ++i)
        {
            GameObject seagul = seaguls[i];

            Vector2 seagulPos = new Vector2(seagul.transform.position.x, seagul.transform.position.z);

            Vector2 center = Vector2.zero;
            Vector2 avgVelocity = Vector2.zero;
            int friends = 0;
            Vector2 avoidance = Vector2.zero;

            for (int j = 0; j < seaguls.Count; ++j)
            {
                GameObject other = seaguls[j];
                if (seagul == other)
                {
                    continue;
                }

                Vector2 otherPos = new Vector2(other.transform.position.x, other.transform.position.z);
                Vector2 otherVel = seagulVelocities[j];

                float dist = Vector2.Distance(seagulPos, otherPos);
                if (dist < vision)
                {
                    center += otherPos;
                    avgVelocity += otherVel;
                    friends++;

                    if (dist < minDist)
                    {
                        avoidance += seagulPos - otherPos;
                    }
                }
            }

            if (friends > 0)
            {
                center /= friends;
                avgVelocity /= friends;

                seagulVelocities[i] += (center - seagulPos) * cohesion * Time.deltaTime;
                seagulVelocities[i] += (avgVelocity - seagulVelocities[i]) * align * Time.deltaTime;
            }

            seagulVelocities[i] += (avoidance * separation) * Time.deltaTime;

            Transform player = GameManager.Get().Player.transform;
            Vector2 playerPos = new (player.position.x, player.position.z);

            if (Vector2.Distance(seagulPos, playerPos) > maxDistanceFromPlayer)
            {
                seagulVelocities[i] += (playerPos - seagulPos).normalized * turnStrength * Time.deltaTime;
            }

            if (seagulVelocities[i].magnitude > maxSpeed)
            {
                seagulVelocities[i] = seagulVelocities[i].normalized * maxSpeed;
            }

            Vector3 velocity = new Vector3(seagulVelocities[i].x, 0, seagulVelocities[i].y);
            seaguls[i].transform.position += velocity * Time.deltaTime;
            //seaguls[i].transform.rotation = Quaternion.LookRotation();
        }
    }
}
