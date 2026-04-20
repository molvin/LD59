using UnityEngine;

public class StartScreen : MonoBehaviour
{
    public float Offset;

    private void Update()
    {
        Boat boat = GameManager.Get().Boat;
        transform.position = boat.transform.position - boat.transform.forward * Offset;
        transform.forward = -boat.transform.forward;
    }
}
