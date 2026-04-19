using UnityEngine;

public class CameraPickup : Pickupable
{
    protected override void Drop()
    {
        GameManager.Get().PolaroidCamera.HasCamera = true;
        Destroy(gameObject);
    }
}
