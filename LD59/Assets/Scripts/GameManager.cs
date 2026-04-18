using UnityEngine;

public class GameManager : Subsystem<GameManager>
{
    private GameObject boat;
    private GameObject player;

    public GameObject Boat => boat;
    public GameObject Player => player;


    private Vector3 startPosition = new(0, 0.5f, 0);


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnGameStart()
    {
        GameManager gameManager = Get();
        gameManager.LoadAssets();
    }

    private void LoadAssets()
    {
        boat = Instantiate(Resources.Load("Boat", typeof(GameObject)) as GameObject, Vector3.zero, Quaternion.identity);
        player = Instantiate(Resources.Load("Player", typeof(GameObject)) as GameObject, startPosition, Quaternion.identity);
    }
}
