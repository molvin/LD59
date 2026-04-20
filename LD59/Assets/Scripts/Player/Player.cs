using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    public enum GroundType
    {
        None,
        Boat,
        Land
    }

    const float PLAYER_HEIGHT = 1.7f;

    [Header("Config")]
    public float MouseSensitivity = 80;
    public float MoveAcceleration = 8;
    public float MoveDeceleration = 0.03f;

    [HideInInspector] public bool MovementEnabled = true;
    [HideInInspector] public bool HoldingPickup = false;
    public bool isSteering { get; private set; }

    private new Camera camera;

    private float rotation;
    private Vector3 localSpaceOffset = new Vector3(0, 0.5f, 0);
    private Vector3 velocity;
    private GroundType standingOn = GroundType.None;
    public GroundType StandingOn => standingOn;
    public Bounds BoatBounds => Utils.GetBounds(GameManager.Get().Boat.gameObject);
    public bool Boated => standingOn == GroundType.Boat;
    private float lastTimeStandingOnBoat;

    public void Reset()
    {
        localSpaceOffset = transform.position - GameManager.Get().Boat.transform.position;
        velocity = Vector3.zero;
    }

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
        if (Input.GetKeyDown(KeyCode.K) && Time.time - lastTimeStandingOnBoat > 120.0f)
        {
            transform.position = GameManager.Get().Boat.transform.position + Vector3.up * 0.5f;
            Reset();
        }

        standingOn = GroundType.None;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit rayHit))
        {
            if (rayHit.collider.gameObject.GetComponentInParent<Boat>() != null)
            {
                standingOn = GroundType.Boat;
                lastTimeStandingOnBoat = Time.time;
            }
            else
            {
                standingOn = GroundType.Land;
            }
        }

        if (MovementEnabled)
        {
            float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

            rotation -= mouseY;
            rotation = Mathf.Clamp(rotation, -90f, 90f);

            camera.transform.localRotation = Quaternion.Euler(rotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1.0f);

            float sprint = Input.GetKey(KeyCode.LeftShift) ? (Boated ? 1.25f : 1.5f) : 1.0f;
            velocity += transform.rotation * input * MoveAcceleration * sprint * Time.deltaTime;
            velocity *= Mathf.Pow(MoveDeceleration, Time.deltaTime);
        }

        Boat boat = GameManager.Get().Boat;

        Vector3 displacement = MovementEnabled && !isSteering ? velocity * Time.deltaTime : Vector3.zero;

        if (Boated)
        {
            Vector3 effectiveWorldPosition = boat.transform.position + boat.transform.rotation * localSpaceOffset;

            displacement += effectiveWorldPosition - transform.position;
            transform.rotation *= Quaternion.AngleAxis(-Mathf.Rad2Deg * boat.DeltaRotation, Vector3.up);
        }

        Vector3 testPos = transform.position + displacement;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(testPos, out hit, 1.0f, NavMesh.AllAreas))
        {
            // Water check
            if (hit.position.y > -0.5f)
            {
                Vector3 normal = (hit.position - testPos).normalized;
                
                float dot = Vector2.Dot(velocity, normal);
                if (dot < 0)
                {
                    velocity -= dot * normal;
                }

                transform.position = hit.position;
            }
            else
            {
                velocity = Vector3.zero;
            }
        }
        else
        {
            velocity = Vector3.zero;
        }

        localSpaceOffset = boat.transform.InverseTransformPoint(transform.position);

        if (isSteering)
        {
            if (Input.GetMouseButtonDown(1))
            {
                ExitSteering();
            }

            float input = 0;
            if (Input.GetKey(KeyCode.W)) input += 1;
            if (Input.GetKey(KeyCode.S)) input -= 1;
            
            if (input != 0)
                boat.Throttle(input);

            input = 0;
            if (Input.GetKey(KeyCode.D)) input += 1;
            if (Input.GetKey(KeyCode.A)) input -= 1;
            
            if(input != 0)
                boat.Steer(input);

            transform.position = boat.SteeringPoint.position;
        }
        else if (Input.GetMouseButton(0) && !GameManager.Get().Book.IsOpen && !GameManager.Get().PolaroidCamera.TakingPicture)
        {
            InteractionSubsystem.Get().Interact(camera.transform);
        }
    }

    public void EnterSteering()
    {
        Boat boat = GameManager.Get().Boat;
        transform.position = boat.SteeringPoint.position;
        isSteering = true;
    }
    private void ExitSteering()
    {
        isSteering = false;
    }
}
