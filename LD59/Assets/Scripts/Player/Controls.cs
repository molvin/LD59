using UnityEngine;

public class Controls : MonoBehaviour
{
    public GameObject WalkingControls;
    public GameObject BoatingControls;
    public GameObject PolaroidControls;
    public GameObject PictureBookControls;
    public GameObject PickupControls;
    public GameObject SignalScopeControls;

    void Start()
    {
        WalkingControls.SetActive(true);
        BoatingControls.SetActive(false);
        PolaroidControls.SetActive(false);
        PictureBookControls.SetActive(false);
        PickupControls.SetActive(false);
        SignalScopeControls.SetActive(false);
    }

    void Update()
    {
        GameManager gm = GameManager.Get();

        BoatingControls.SetActive(gm.Player.isSteering);
        PolaroidControls.SetActive(gm.PolaroidCamera.TakingPicture);
        PictureBookControls.SetActive(gm.Book.IsOpen && !gm.Player.HoldingPickup);
        PickupControls.SetActive(gm.Player.HoldingPickup);
        SignalScopeControls.SetActive(gm.SignalScope.Enabled);

        WalkingControls.SetActive(!gm.Player.isSteering && !gm.PolaroidCamera.TakingPicture && !gm.Book.IsOpen && !gm.Player.HoldingPickup && !gm.SignalScope.Enabled);
    }
}
