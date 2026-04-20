using UnityEngine;

public class Pickupable : Interactable
{
    public float HoldDistance = 1.5f;
    public float RotateSpeed = 90f;
    public bool Interactable = true;

    private bool holding;
    private Player player;
    private Camera cam;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
    }

    protected override void Interact(Transform interactorTransform)
    {
        if (!Interactable)
            return;

        cam = Camera.main;
        player = FindFirstObjectByType<Player>();
        player.MovementEnabled = false;
        player.HoldingPickup = true;

        transform.SetParent(null);
        holding = true;

        transform.position = cam.transform.position + cam.transform.forward * HoldDistance;
        transform.forward = -cam.transform.forward;
    }

    private void LateUpdate()
    {
        if (!holding)
            return;

        transform.position = cam.transform.position + cam.transform.forward * HoldDistance;

        float rotX = 0f, rotY = 0f;
        if (Input.GetKey(KeyCode.W)) rotX += 1f;
        if (Input.GetKey(KeyCode.S)) rotX -= 1f;
        if (Input.GetKey(KeyCode.A)) rotY += 1f;
        if (Input.GetKey(KeyCode.D)) rotY -= 1f;

        transform.Rotate(cam.transform.up, rotY * RotateSpeed * Time.deltaTime, Space.World);
        transform.Rotate(cam.transform.right, rotX * RotateSpeed * Time.deltaTime, Space.World);

        if (Input.GetMouseButtonDown(1))
        {
            holding = false;
            player.MovementEnabled = true;
            player.HoldingPickup = false;
            Drop();
        }
    }

    protected virtual void Drop()
    {
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}
