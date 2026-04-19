using Unity.VisualScripting;
using UnityEngine;

public class GameManager : Subsystem<GameManager>
{
    private Boat boat;
    private Player player;
    private PolaroidBook book;
    private PolaroidCamera polaroidCamera;

    public Boat Boat => boat;
    public Player Player => player;
    public PolaroidBook Book => book;
    public PolaroidCamera PolaroidCamera => polaroidCamera;


    private Vector3 startPosition = new(0, 0.5f, 0);
    private Quaternion startRotation = Quaternion.identity;


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
    }
}
