using UnityEngine;

public class CameraPickup : Pickupable
{
    public Canvas TutorialCanvas;

    private void Update()
    {
        TutorialCanvas.gameObject.SetActive(holding);
        Transform camT = Camera.main.transform;
        TutorialCanvas.transform.forward = -camT.forward;
    }

    protected override void Drop()
    {
        GameManager.Get().PolaroidCamera.HasCamera = true;
        Destroy(gameObject);
    }
}
