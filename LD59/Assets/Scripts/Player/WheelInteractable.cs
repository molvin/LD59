using UnityEngine;

public class WheelInteractable : Interactable
{
    protected override void Interact(Transform interactorTransform)
    {
        GameManager.Get().Player.EnterSteering();
    }
}
