using UnityEngine;

public class GoldenDoor : MonoBehaviour
{
    public GameObject GoldenDoorObject;
    public float ActivationDistance = 10;

    void Update()
    {
        if (GameManager.Get().HasGameBeenWon)
        {
            GoldenDoorObject.SetActive(false);

            if (Vector3.Distance(GameManager.Get().Player.transform.position, GoldenDoorObject.transform.position) < ActivationDistance)
            {
                Debug.Log("YOU'RE WINNER!");
            }
        }
    }
}
