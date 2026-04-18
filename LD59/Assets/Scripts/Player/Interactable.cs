using System;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public float InteractDistance = 1.0f;

    public Action<float> OnInteracted;

    public void TryInteract(Vector3 interactionPoint, Vector3 lookDirection, float action)
    {
        Vector3 toObject = transform.position - interactionPoint;

        // Occlusion test
        RaycastHit hit;
        if (Physics.Raycast(interactionPoint, toObject.normalized, out hit, toObject.magnitude))
        {
            if (hit.collider.gameObject != gameObject)
            {
                return;
            }
        }

        // Within interaction range
        float angle = Mathf.Deg2Rad * Vector3.Angle(toObject, lookDirection);
        float reach = toObject.magnitude / Mathf.Cos(angle);
        if (reach < InteractDistance)
        {
            OnInteracted?.Invoke(action);
        }
    }
}
