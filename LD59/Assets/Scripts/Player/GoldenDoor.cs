using UnityEngine;

public class GoldenDoor : MonoBehaviour
{
    public GameObject GoldenDoorObject;
    public GameObject Portal;
    public GameObject Credits;
    public float ActivationDistance = 10;

    void Update()
    {
        if (GameManager.Get().HasGameBeenWon)
        {
            GoldenDoorObject.SetActive(false);

            if (Vector3.Distance(GameManager.Get().Player.transform.position, GoldenDoorObject.transform.position) < ActivationDistance)
            {
Portal.SetActive(true);
                Credits.SetActive(true);
            }
        }
    }
}
