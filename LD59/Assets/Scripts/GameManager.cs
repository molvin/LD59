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
        boat = Instantiate(Resources.Load("Boat") as GameObject, Vector3.zero, Quaternion.identity).GetComponent<Boat>();
        player = Instantiate(Resources.Load("Player") as GameObject, startPosition, Quaternion.identity).GetComponent<Player>();
    
        book = FindFirstObjectByType<PolaroidBook>(FindObjectsInactive.Include);
        polaroidCamera = FindFirstObjectByType<PolaroidCamera>(FindObjectsInactive.Include);
    }
}
