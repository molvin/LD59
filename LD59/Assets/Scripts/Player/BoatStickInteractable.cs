using UnityEngine;

public class BoatStickInteractable : Interactable
{
    public override void Interact(Transform interactorTransform)
    {
        GameManager.Get().Player.EnterSteering();
    }
}
