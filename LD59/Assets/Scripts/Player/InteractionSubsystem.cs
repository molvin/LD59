using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class InteractionSubsystem : Subsystem<InteractionSubsystem>
{
    private List<Interactable> interactables = new();

    public void Interact(Transform interactorTransform)
    {
        for (int i = interactables.Count - 1; i >= 0; --i)
        {
            if (interactables[i] == null)
            {
                interactables.RemoveAtSwapBack(i);
            }
            else
            {
                interactables[i].TryInteract(interactorTransform);
            }
        }
    }

    public void Register(Interactable interactable)
    {
        interactables.Add(interactable);
    }
}
