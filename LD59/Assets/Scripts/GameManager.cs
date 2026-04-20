using System.Collections.Generic;
using UnityEngine;

public class GameManager : Subsystem<GameManager>
{
    public bool HasGameBeenWon => happyPillars.Count >= 3;
    private bool isDay = true;
    public bool IsDay => isDay;

    private Boat boat;
    private Player player;
    private PolaroidBook book;
    private PolaroidCamera polaroidCamera;
    private SignalScope signalScope;

    public Boat Boat => boat;
    public Player Player => player;
    public PolaroidBook Book => book;
    public PolaroidCamera PolaroidCamera => polaroidCamera;
    public SignalScope SignalScope => signalScope;


    private Vector3 startPosition = new(0, 0.5f, 0);
    private Quaternion startRotation = Quaternion.identity;
    public List<GameObject> happyPillars = new();
    private List<DayNightBound> timeBound = new();

    public int HappyPillarCount => happyPillars.Count;

    public void ToggleDayNight()
    {
        isDay = !isDay;

        foreach (var thing in timeBound)
        {
            thing.gameObject.SetActive((thing.DayBound && isDay) || (!thing.DayBound && !isDay));
        }
    }

    public void RegisterHappyPillar(GameObject pillar)
    {
        if (!happyPillars.Contains(pillar))
        {
            happyPillars.Add(pillar);
        }
    }

    public void RegisterDayNightBound(DayNightBound thing)
    {
        timeBound.Add(thing);
        thing.gameObject.SetActive((thing.DayBound && isDay) || (!thing.DayBound && !isDay));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnGameStart()
    {
        if (!SceneConfig.InitializePlayScene())
        {
            return;
        }

        GameManager gameManager = Get();
        gameManager.LoadAssets();
    }

    private void LoadAssets()
    {
        StartPoint startPoint = FindFirstObjectByType<StartPoint>();

        if(startPoint != null)
        {
            Debug.Log($"SPAWNING AT: {startPoint.transform.position}");
            startPosition = startPoint.transform.position;
            startRotation = startPoint.transform.rotation;
        }

        boat = Instantiate(Resources.Load("Boat") as GameObject, startPosition, startRotation).GetComponent<Boat>();
        player = Instantiate(Resources.Load("Player") as GameObject, startPosition, startRotation).GetComponent<Player>();
    
        book = FindFirstObjectByType<PolaroidBook>(FindObjectsInactive.Include);
        polaroidCamera = FindFirstObjectByType<PolaroidCamera>(FindObjectsInactive.Include);
        signalScope = FindFirstObjectByType<SignalScope>(FindObjectsInactive.Include);
    }
}
