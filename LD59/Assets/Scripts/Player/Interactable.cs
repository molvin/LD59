using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public float InteractDistance = 1.6f;
    public float InteractError = 0.4f;


    private void OnEnable()
    {
        InteractionSubsystem.Get().Register(this);
    }

    public abstract void Interact(Transform interactorTransform);

    public void TryInteract(Transform interactorTransform)
    {
        Vector3 toObject = transform.position - interactorTransform.position;

        if (toObject.sqrMagnitude > InteractDistance * InteractDistance)
        {
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(interactorTransform.position, interactorTransform.forward, out hit, toObject.magnitude))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Interact(interactorTransform);
            }
        }

        if (Physics.Raycast(interactorTransform.position, toObject.normalized, out hit, toObject.magnitude))
        {
            if (hit.collider.gameObject != gameObject)
            {
                return;
            }
        }

        // Within interaction range
        float angle = Mathf.Deg2Rad * Vector3.Angle(toObject, interactorTransform.forward);
        float reach = toObject.magnitude * Mathf.Tan(angle);
        if (Mathf.Abs(reach) < InteractError)
        {
            Interact(interactorTransform);
        }
    }
}
