using UnityEngine;

public class WheelInteractable : Interactable
{
    private Boat boat;

    public void Start()
    {
        boat = GetComponentInParent<Boat>();
    }

    protected override void Interact(Transform interactorTransform)
    {
        float toWheel = Vector3.Dot((transform.position - interactorTransform.position).normalized, boat.transform.right);
        float lookAtWheel = Vector3.Dot(interactorTransform.forward, boat.transform.right);
        float action = lookAtWheel > toWheel ? 1.0f : -1.0f;
        boat.Steer(action);
    }
}
