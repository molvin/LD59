using System.Collections.Generic;
using UnityEngine;

public class Dropoff : MonoBehaviour
{
    [SerializeField] private float MinDistance;
    [SerializeField] private float MaxDistance;
    [SerializeField] private float MaxDropoff;
    [SerializeField] private List<GameObject> DropoffList = new();

    private List<Vector3> startPositions = new();
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        foreach (GameObject dropOff in DropoffList)
        {
            startPositions.Add(dropOff.transform.position);
        }
    }

    public void AddToDropOff(GameObject gameObject)
    {
        DropoffList.Add(gameObject);
        startPositions.Add(gameObject.transform.position);
    }

    private void Update()
    {
        for (int i = 0; i < DropoffList.Count; i++)
        {
            Vector3 camXZ = mainCamera.transform.position;
            camXZ.y = 0.0f;

            GameObject dropoff = DropoffList[i];
            Vector3 objXZ = dropoff.transform.position;
            objXZ.y = 0.0f;

            float distanceToCamera = Vector3.Distance(camXZ, objXZ);
            float t = Mathf.Clamp01((distanceToCamera - MinDistance) / (MaxDistance - MinDistance));
            dropoff.transform.position = startPositions[i] + Vector3.down * Mathf.Lerp(0, MaxDropoff, t);
        }
    }
}
