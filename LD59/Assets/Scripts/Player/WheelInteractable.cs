using UnityEngine;

public class WheelInteractable : Interactable
{
    public override void Interact(Transform interactorTransform)
    {
        GameManager.Get().Player.EnterSteering();
    }
}
