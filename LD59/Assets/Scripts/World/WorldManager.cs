using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    private GameObject seagullPrefab;
    private GameObject seagullHolder;
    private GameObject koiPrefab;
    private GameObject koiHolder;
    private List<GameObject> kois = new();

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

        seagullPrefab = Resources.Load("Seagul") as GameObject;
        seagullHolder = new GameObject("Seagulls");

        koiPrefab = Resources.Load("koi") as GameObject;
        koiHolder = new GameObject("Kois");
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
        MakeFishesFish();
    }

    private void SeagulSim()
    {
        Transform player = GameManager.Get().Player.transform;

        if (seaguls.Count < 40)
        {
            GameObject seagul = Instantiate(seagullPrefab, seagullHolder.transform);
            Vector3 pos = Random.onUnitSphere * 100;
            pos.y = pos.y * 0.15f + 30;
            seagul.transform.position = player.transform.position + pos;
            seaguls.Add(seagul);
            seagulVelocities.Add(Random.onUnitCircle * 5);
        }

        foreach (GameObject seagull in seaguls)
        {
            Vector3 toSeagull = seagull.transform.position - player.transform.position;

            Vector3 relativeSeagull = Vector3.zero;
            if (toSeagull.x < -200)
            {
                relativeSeagull.x += 300;
            }
            if (toSeagull.x > 200)
            {
                relativeSeagull.x -= 300;
            }
            if (toSeagull.z < -200)
            {
                relativeSeagull.z += 300;
            }
            if (toSeagull.z > 200)
            {
                relativeSeagull.z -= 300;
            }

            seagull.transform.position += relativeSeagull;
        }

        float vision = 30;
        float minDist = 13;
        float maxSpeed = 4;
        float cohesion = 0.12f;
        float align = 0.03f;
        float separation = 1.0f;
        float rngSpeed = 0.9f;

        for (int i = 0; i < seaguls.Count; ++i)
        {
            GameObject seagul = seaguls[i];
            Vector2 rng = Random.onUnitCircle;
            seagulVelocities[i] += rng * rngSpeed * Time.deltaTime;

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

            if (seagulVelocities[i].magnitude > maxSpeed)
            {
                seagulVelocities[i] = seagulVelocities[i].normalized * maxSpeed;
            }

            Vector3 velocity = new Vector3(seagulVelocities[i].x, 0, seagulVelocities[i].y);
            seaguls[i].transform.position += velocity * Time.deltaTime;
            seaguls[i].transform.forward = velocity.normalized;
        }
    }
    
    private void MakeFishesFish()
    {
        Player player = GameManager.Get().Player;
        if (kois.Count < 200)
        {
            GameObject newKoi = Instantiate(koiPrefab, koiHolder.transform);
            Vector2 rng = Random.onUnitCircle * (1.0f + Random.value) * 40.0f;
            newKoi.transform.position = player.transform.position + new Vector3(rng.x, -Random.value * 2.0f - 1.0f, rng.y);
            newKoi.transform.localScale = new (Random.value > 0.5 ? 1 : -1, 1, 1);
            kois.Add(newKoi);
        }

        Boat boat = GameManager.Get().Boat;
        foreach (GameObject koi in kois)
        {
            if (Vector3.Distance(boat.transform.position, koi.transform.position) > 100)
            {
                if (Vector3.Dot(boat.transform.forward, (boat.transform.position - koi.transform.position)) > 0)
                {
                    koi.transform.position += boat.transform.forward * 200;
                    Vector2 dir = Random.onUnitCircle;
                    koi.transform.forward = new(dir.x, 0, dir.y);
                }
            }

            koi.transform.position += koi.transform.forward * (1.0f + Random.value) * Time.deltaTime;
        }

        //foreach (GameObject koi in kois)
        //{
            //Vector2 targetPos2D = new Vector2(player.transform.position.x, player.transform.position.z);// + Random.onUnitCircle * 20.0f;
            //Vector3 targetPos = new(targetPos2D.x, koi.transform.position.y, targetPos2D.y);
            //Vector3 toTarget = (targetPos - koi.transform.position).normalized;
            //Vector3 dir = koi.transform.localScale.x * Vector3.Cross(toTarget, Vector3.up);
            //targetPos += dir * Random.value * 100.0f;
            //dir = (targetPos - koi.transform.position).normalized;
            //dir = Vector3.RotateTowards(koi.transform.forward, dir, Time.deltaTime, Time.deltaTime);
            //koi.transform.forward = dir;
            //koi.transform.position += dir * 20.0f * Random.value * Time.deltaTime;
        //}
    }
}
