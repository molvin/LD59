using UnityEngine;

public class CameraPickup : Pickupable
{
    public Canvas TutorialCanvas;

    private void Update()
    {
        TutorialCanvas.gameObject.SetActive(holding);
        Transform camT = Camera.main.transform;
        TutorialCanvas.transform.forward = -camT.forward;

        if (Input.GetKeyDown(KeyCode.T) && holding)
        {
            holding = false;
            if (ControlPlayerMovement)
            {
                Player player = GameManager.Get().Player;
                player.MovementEnabled = true;
                player.HoldingPickup = false;
            }
            Drop();
            var c = GameManager.Get().PolaroidCamera;
            c.StartCoroutine(c.TakePicture());
        }
    }

    protected override void Drop()
    {
        GameManager.Get().PolaroidCamera.HasCamera = true;
        Destroy(gameObject);
    }
}
