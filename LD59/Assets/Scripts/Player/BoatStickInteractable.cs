using UnityEngine;

public class BoatStickInteractable : Interactable
{
    protected override void Interact(Transform interactorTransform)
    {
        GameManager.Get().Player.EnterSteering();
    }
}
