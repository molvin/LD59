using UnityEngine;

public class Pickupable : Interactable
{
    public float HoldDistance = 1.5f;
    public float RotateSpeed = 90f;

    private bool holding;
    private Player player;
    private Camera cam;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    protected override void Interact(Transform interactorTransform)
    {
        cam = Camera.main;
        player = FindFirstObjectByType<Player>();
        player.MovementEnabled = false;

        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;

        transform.SetParent(null);
        holding = true;
    }

    private void Update()
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
            transform.SetParent(originalParent);
            transform.SetPositionAndRotation(originalPosition, originalRotation);
            player.MovementEnabled = true;
        }
    }
}
