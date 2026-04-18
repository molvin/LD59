using UnityEngine;

public class Player : MonoBehaviour
{
    const float PLAYER_HEIGHT = 1.7f;

    [Header("Config")]
    public float MouseSensitivity = 80;
    public float MovementSpeed = 2;

    private new Camera camera;

    private float rotation;
    private bool boated = true;
    public bool MovementEnabled = true;
    public Bounds BoatBounds => Utils.GetBounds(GameManager.Get().Boat.gameObject);

    void Start()
    {
        camera = Camera.main;
        if (camera == null)
        {
            camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
        }
        camera.transform.SetParent(transform);
        camera.transform.SetLocalPositionAndRotation(new (0, PLAYER_HEIGHT, 0), Quaternion.LookRotation(transform.forward, Vector3.up));

        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        if (MovementEnabled)
        {
            float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

            rotation -= mouseY;
            rotation = Mathf.Clamp(rotation, -90f, 90f);

            camera.transform.localRotation = Quaternion.Euler(rotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            Vector3 input = new(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1.0f);

            transform.position += transform.rotation * input * MovementSpeed * Time.deltaTime;
        }

        if (boated)
        {
            Boat boat = GameManager.Get().Boat;

            if (MovementEnabled && Input.GetMouseButton(0))
            {
                InteractionSubsystem.Get().Interact(camera.transform);
            }

            transform.position += boat.DeltaVelocity;
            transform.rotation *= Quaternion.AngleAxis(-Mathf.Rad2Deg * boat.DeltaRotation, Vector3.up);

            Bounds boatBounds = BoatBounds;

            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, boatBounds.min.x, boatBounds.max.x);
            clampedPos.y = boat.DeckLevel;
            clampedPos.z = Mathf.Clamp(clampedPos.z, boatBounds.min.z, boatBounds.max.z);

            transform.position = clampedPos;
        }
    }
}
