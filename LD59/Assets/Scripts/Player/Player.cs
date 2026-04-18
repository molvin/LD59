using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;

public class Player : MonoBehaviour
{
    const float PLAYER_HEIGHT = 1.7f;

    [Header("Config")]
    public float MouseSensitivity = 80;
    public float MoveAcceleration = 8;
    public float MoveDeceleration = 0.03f;

    private new Camera camera;

    private float rotation;
    private bool boated = true;
    private Vector3 localSpaceOffset = new Vector3(0, 0.5f, 0);
    private Vector3 velocity;
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
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        rotation -= mouseY;
        rotation = Mathf.Clamp(rotation, -90f, 90f);

        camera.transform.localRotation = Quaternion.Euler(rotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        input = Vector3.ClampMagnitude(input, 1.0f);

        velocity += transform.rotation * input * MoveAcceleration * Time.deltaTime;
        velocity *= Mathf.Pow(MoveDeceleration, Time.deltaTime);

        Vector3 displacement = velocity * Time.deltaTime;

        if (boated)
        {
            Boat boat = GameManager.Get().Boat;

            Vector3 effectiveWorldPosition = boat.transform.position + boat.transform.rotation * localSpaceOffset;

            displacement += effectiveWorldPosition - transform.position;
            transform.rotation *= Quaternion.AngleAxis(-Mathf.Rad2Deg * boat.DeltaRotation, Vector3.up);
        }

        transform.position += displacement;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            // TODO: Normal force
            transform.position = hit.position;
        }
        else
        {
            velocity = Vector3.zero;
        }

        if (boated)
        {
            Boat boat = GameManager.Get().Boat;
            localSpaceOffset = boat.transform.InverseTransformPoint(transform.position);
        }

        if (Input.GetMouseButton(0))
        {
            InteractionSubsystem.Get().Interact(camera.transform);
        }
    }
}
