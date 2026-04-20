using FMODUnity;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.AI;

public class Boat : MonoBehaviour
{
    struct RayCollisionSettings
    {
        public float verticalWidth;
        public float horizontalWidth;
        public float forward;
        public float back;
        public float right;
        public float left;
    }

    [Header("References")]
    public Interactable Wheel;
    public float DeckLevel = 0.5f;
    [Header("Steering")]
    public float Acceleration = 3.5f;
    public float Deceleration = 0.35f;
    public float TurnSpeed = 0.6f;
    public float WheelSpeed = 0.4f;
    public float WheelReset = 0.8f;
    public float GearSpeed = 0.6f;
    public float driftNoiseScale = 0.01f;
    public float driftStrength = 0.2f;

    [Header("Audio")]
    public StudioEventEmitter engineNoise;
    public StudioEventEmitter boatSplashNoise;
    public float MaxSpeed = 20f;

    [Header("Collision")]
    public LayerMask layerMask;

    [Header("Parking Assistant")]
    public float ParkingDistanceCheck;
    public float ParkingRotation;
    public float ParkingLinear;

    public Transform SteeringPoint;
    public Transform ThrottlePivot;

    private float throttle = 0.0f;
    private float steering = 0.0f;

    private Vector2 linearVelocity;
    private float angularVelocity;
    private Vector3 deltaVelocity;
    private float deltaRotation;
    private bool isSteering = false;
    private bool isThrottling = false;
    private RayCollisionSettings rayCollisionSettings;
    public Transform StartPosition;
    public Vector3 DeltaVelocity => deltaVelocity;
    public float DeltaRotation => deltaRotation;

    public void Stop()
    {
        linearVelocity = Vector2.zero;
        angularVelocity = 0;
        throttle = 0;
        steering = 0;
    }

    public void Throttle(float input)
    {
        float throttleJerk = input * GearSpeed * Time.deltaTime;
        throttle = Mathf.Clamp(throttle + throttleJerk, -1.0f, 1.0f);

        isThrottling = true;
    }

    public void Steer(float input)
    {
        float turnJerk = input * WheelSpeed * Time.deltaTime;
        steering = Mathf.Clamp(steering + turnJerk, -1.0f, 1.0f);

        isSteering = true;
    }

    void Start()
    {
        transform.rotation = Quaternion.identity;
        Bounds bounds = Utils.GetBounds(gameObject);
        rayCollisionSettings = new RayCollisionSettings
        {
            verticalWidth = Mathf.Abs(bounds.max.x - bounds.min.x) * 0.5f,
            horizontalWidth = Mathf.Abs(bounds.max.z - bounds.min.z) * 0.5f,
            forward = Mathf.Abs(bounds.max.z - transform.position.z),
            back = Mathf.Abs(transform.position.z - bounds.min.z),
            right = Mathf.Abs(bounds.max.x - transform.position.x),
            left = Mathf.Abs(transform.position.x - bounds.min.x),
        };
    }

    void Update()
    {
        for (int i = 0; i < 10; i++)
        {
            float alpha = (float)i / 10.0f;

            Vector3 vertPos = transform.position + transform.right * ((-rayCollisionSettings.verticalWidth * 0.5f) + (alpha * rayCollisionSettings.verticalWidth));
            Debug.DrawLine(vertPos, vertPos + transform.forward * rayCollisionSettings.forward, Color.blue);

            Vector3 horizPos = transform.position + transform.forward * ((-rayCollisionSettings.horizontalWidth * 0.5f) + (alpha * rayCollisionSettings.horizontalWidth));
            Debug.DrawLine(horizPos, horizPos + transform.right * rayCollisionSettings.right, Color.red);
        }

        if (GameManager.Get().Player.StandingOn == Player.GroundType.Boat)
        {
            Drift();
        }

        ParkingAssistant();

        Vector3 currentPosition = transform.position;
        Vector3 currentPlaneForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        if (!isSteering)
        {
            steering *= Mathf.Pow(WheelReset, Time.deltaTime);
        }
        isSteering = false;
        Wheel.transform.localRotation = Quaternion.Euler(-300 * steering, 0, 0);

        if (!isThrottling && throttle < 0.16f && throttle > -0.32f)
        {
            throttle *= Mathf.Pow(WheelReset, Time.deltaTime);
        }
        isThrottling = false;
        float t = (throttle + 1.0f) / 2.0f;
        ThrottlePivot.localRotation = Quaternion.Euler(Mathf.Lerp(-113, -65, t), 90 , -90);

        float speed = Vector3.Dot(new Vector3(linearVelocity.x, 0, linearVelocity.y), transform.forward);
        // NOTE: Science
        float turningAlpha = Mathf.Clamp01((2.0f + Mathf.Abs(speed)) / 6.8f);
        turningAlpha *= Mathf.Sign(speed + 1);

        angularVelocity += turningAlpha * steering * Mathf.Rad2Deg * TurnSpeed * Time.deltaTime;
        angularVelocity *= Mathf.Pow(Deceleration, Time.deltaTime);
        transform.Rotate(Vector3.up, angularVelocity * Time.deltaTime);

        float throttleJuice = throttle > 0 ? 1.0f : 0.5f;
        linearVelocity += new Vector2(transform.forward.x, transform.forward.z) * (throttle * throttleJuice * Acceleration * Time.deltaTime);
        linearVelocity *= Mathf.Pow(Deceleration, Time.deltaTime);

        transform.position += new Vector3(linearVelocity.x, 0, linearVelocity.y) * Time.deltaTime;

        deltaVelocity = transform.position - currentPosition;
        deltaRotation = Mathf.Deg2Rad * Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized, currentPlaneForward, Vector3.up);

        engineNoise.SetParameter("RPM", Mathf.Abs(throttle));

        void NormalForce(Vector2 normal)
        {
            float dot = Vector2.Dot(linearVelocity, normal);

            if (dot < 0)
            {
                linearVelocity -= dot * normal;
            }
        }

        // Forward
        RaycastHit rayHit;
        if (Physics.BoxCast(transform.position, new Vector3(rayCollisionSettings.verticalWidth * 0.5f, 0.1f, 0.01f),
            transform.forward, out rayHit, transform.rotation, rayCollisionSettings.forward, layerMask))
        {
            NormalForce(new (rayHit.normal.x, rayHit.normal.z));

            transform.position -= transform.forward * (rayCollisionSettings.forward - rayHit.distance);
        }
        // Back
        if (Physics.BoxCast(transform.position, new Vector3(rayCollisionSettings.verticalWidth * 0.5f, 0.1f, 0.01f),
            -transform.forward, out rayHit, transform.rotation, rayCollisionSettings.back, layerMask))
        {
            NormalForce(new (rayHit.normal.x, rayHit.normal.z));

            transform.position += transform.forward * (rayCollisionSettings.back - rayHit.distance);
        }
        // Right
        if (Physics.BoxCast(transform.position, new Vector3(0.01f, 0.1f, rayCollisionSettings.horizontalWidth * 0.5f),
            transform.right, out rayHit, transform.rotation, rayCollisionSettings.right, layerMask))
        {
            NormalForce(new (rayHit.normal.x, rayHit.normal.z));

            transform.position -= transform.right * (rayCollisionSettings.right - rayHit.distance);
        }
        // Left
        if (Physics.BoxCast(transform.position, new Vector3(0.01f, 0.1f, rayCollisionSettings.horizontalWidth * 0.5f),
            -transform.right, out rayHit, transform.rotation, rayCollisionSettings.left, layerMask))
        {
            NormalForce(new (rayHit.normal.x, rayHit.normal.z));

            transform.position += transform.right * (rayCollisionSettings.left - rayHit.distance);
        }
        boatSplashNoise.SetParameter("Speed", Mathf.Clamp01(linearVelocity.magnitude / MaxSpeed));
    }

    private void Drift()
    {
        float xCoord = (transform.position.x + Time.time) * driftNoiseScale;
        float zCoord = (transform.position.z + Time.time) * driftNoiseScale;

        float noiseSample = Mathf.PerlinNoise(xCoord, zCoord);

        float angle = noiseSample * Mathf.PI * 2f;

        linearVelocity += new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * driftStrength * Time.deltaTime;
        angularVelocity += angle * driftStrength * Time.deltaTime;
    }

    private void ParkingAssistant()
    {
        float rotationAssistance = 0;
        Vector2 velocityAssistance = Vector2.zero;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.2f, transform.right, out hit, ParkingDistanceCheck, layerMask))
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(hit.point, out navHit, 1.0f, NavMesh.AllAreas))
            {
                Vector3 toHit = hit.point - transform.position;
                float rotation = Vector3.Dot(transform.forward, hit.normal);

                Debug.DrawLine(transform.position, hit.point, Color.rebeccaPurple, 0.2f);

                rotationAssistance += rotation * ParkingRotation;
                velocityAssistance += new Vector2(toHit.x, toHit.z).normalized * ParkingLinear;
            }
        }
        if (Physics.SphereCast(transform.position, 0.2f, -transform.right, out hit, ParkingDistanceCheck, layerMask))
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(hit.point, out navHit, 1.0f, NavMesh.AllAreas))
            {
                Vector3 toHit = hit.point - transform.position;
                float rotation = -Vector3.Dot(transform.forward, hit.normal);

                Debug.DrawLine(transform.position, hit.point, Color.rebeccaPurple, 0.2f);

                rotationAssistance += rotation * ParkingRotation;
                velocityAssistance += new Vector2(toHit.x, toHit.z).normalized * ParkingLinear;
            }
        }

        angularVelocity += rotationAssistance * Time.deltaTime;
        linearVelocity += velocityAssistance * Time.deltaTime;
    }
}