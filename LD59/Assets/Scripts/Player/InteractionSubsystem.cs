using System.Collections.Generic;
using UnityEngine;

public class InteractionSubsystem : Subsystem<InteractionSubsystem>
{
    private List<Interactable> interactables = new();

    public void Interact(Transform interactorTransform)
    {
        foreach (Interactable interactable in interactables)
        {
            interactable.TryInteract(interactorTransform);
        }
    }

    public void Register(Interactable interactable)
    {
        interactables.Add(interactable);
    }

    public void Unregister(Interactable interactable)
    {
        interactables.Remove(interactable);
    }
}
