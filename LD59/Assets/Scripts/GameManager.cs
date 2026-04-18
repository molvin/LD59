using UnityEngine;

public class GameManager : Subsystem<GameManager>
{
    private Boat boat;
    private Player player;

    public Boat Boat => boat;
    public Player Player => player;


    private Vector3 startPosition = new(0, 0.5f, 0);


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnGameStart()
    {
        GameManager gameManager = Get();
        gameManager.LoadAssets();
    }

    private void LoadAssets()
    {
        boat = Instantiate(Resources.Load("Boat") as GameObject, Vector3.zero, Quaternion.identity).GetComponent<Boat>();
        player = Instantiate(Resources.Load("Player") as GameObject, startPosition, Quaternion.identity).GetComponent<Player>();
    }
}
