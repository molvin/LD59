using UnityEngine;

public class BoatStickInteractable : Interactable
{
    private Boat boat;

    public void Start()
    {
        boat = GetComponentInParent<Boat>();
    }

    protected override void Interact(Transform interactorTransform)
    {
        float toStick = Vector3.Dot((transform.position - interactorTransform.position).normalized, boat.transform.up);
        float lookAtStick = Vector3.Dot(interactorTransform.forward, boat.transform.up);
        float action = lookAtStick > toStick ? 1.0f : -1.0f;
        boat.Throttle(action);
    }
}
