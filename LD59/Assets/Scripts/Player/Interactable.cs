using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public float InteractDistance = 1.6f;
    public float InteractError = 0.4f;

    public bool CanBeInteractedWith = true;


    private void OnEnable()
    {
        InteractionSubsystem.Get().Register(this);
    }

    public abstract void Interact(Transform interactorTransform);

    public bool CanInteract(Transform interactorTransform)
    {
        if (!CanBeInteractedWith)
            return false;

        Vector3 toObject = transform.position - interactorTransform.position;

        if (toObject.sqrMagnitude > InteractDistance * InteractDistance)
        {
            return false;
        }

        RaycastHit hit;
        if (Physics.Raycast(interactorTransform.position, interactorTransform.forward, out hit, toObject.magnitude))
        {
            if (hit.collider.gameObject == gameObject)
            {
                return true;
            }
        }

        if (Physics.Raycast(interactorTransform.position, toObject.normalized, out hit, toObject.magnitude))
        {
            if (hit.collider.gameObject != gameObject)
            {
                return false;
            }
        }

        // Within interaction range
        float angle = Mathf.Deg2Rad * Vector3.Angle(toObject, interactorTransform.forward);
        float reach = toObject.magnitude * Mathf.Tan(angle);
        if (Mathf.Abs(reach) < InteractError)
        {
            return true;
        }

        return false;
    }

    public void TryInteract(Transform interactorTransform)
    {
        if(CanInteract(interactorTransform))
        {
            Interact(interactorTransform);
        }
    }
}
