using FMODUnity;
using Unity.Mathematics;
using UnityEngine;

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

    [Header("Audio")]
    public StudioEventEmitter engineNoise;
    public StudioEventEmitter boatSplashNoise;
    public float MaxSpeed = 20f;

    [Header("Collision")]
    public LayerMask layerMask;

    public Transform SteeringPoint;
    public Transform ThrottlePivot;

    private float throttle = 0.0f;
    private float steering = 0.0f;

    private Vector2 linearVelocity;
    private float angularVelocity;
    private Vector3 deltaVelocity;
    private float deltaRotation;
    private bool isSteering = false;
    private RayCollisionSettings rayCollisionSettings;

    public Vector3 DeltaVelocity => deltaVelocity;
    public float DeltaRotation => deltaRotation;

    public void Throttle(float input)
    {
        float throttleJerk = input * GearSpeed * Time.deltaTime;
        if (throttleJerk < 0.0f)
        {
            throttleJerk *= 0.5f;
        }
        throttle = Mathf.Clamp(throttle + throttleJerk, -1.0f, 1.0f);

        float t = (throttle + 1.0f) / 2.0f;
        ThrottlePivot.localRotation = Quaternion.Euler(Mathf.Lerp(-135, -50, t), 90 , -90);
    }

    public void Steer(float input)
    {
        float turnJerk = input * WheelSpeed * Time.deltaTime;
        steering = Mathf.Clamp(steering + turnJerk, -1.0f, 1.0f);

        isSteering = true;
    }

    void Start()
    {
        // transform.rotation = Quaternion.identity;
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

        Vector3 currentPosition = transform.position;
        Vector3 currentPlaneForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        if (!isSteering)
        {
            steering *= Mathf.Pow(WheelReset, Time.deltaTime);
        }
        isSteering = false;
        Wheel.transform.localRotation = Quaternion.Euler(-300 * steering, 0, 0);
        //Wheel.transform.rotation = Quaternion.AngleAxis(300 * steering, Vector3.forward);

        angularVelocity += steering * Mathf.Rad2Deg * TurnSpeed * Time.deltaTime;
        angularVelocity *= Mathf.Pow(Deceleration, Time.deltaTime);
        transform.Rotate(Vector3.up, angularVelocity * Time.deltaTime);

        linearVelocity += new Vector2(transform.forward.x, transform.forward.z) * (throttle * Acceleration * Time.deltaTime);
        linearVelocity *= Mathf.Pow(Deceleration, Time.deltaTime);
        transform.position += new Vector3(linearVelocity.x, 0, linearVelocity.y) * Time.deltaTime;

        deltaVelocity = transform.position - currentPosition;
        deltaRotation = Mathf.Deg2Rad * Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized, currentPlaneForward, Vector3.up);

        engineNoise.SetParameter("RPM", Mathf.Abs(throttle));

        // Forward
        RaycastHit rayHit;
        if (Physics.BoxCast(transform.position, new Vector3(rayCollisionSettings.verticalWidth * 0.5f, 10.0f, 1.0f),
            transform.forward, out rayHit, transform.rotation, rayCollisionSettings.forward, layerMask))
        {
            transform.position -= transform.forward * (rayCollisionSettings.forward - rayHit.distance);
        }
        // Back
        if (Physics.BoxCast(transform.position, new Vector3(rayCollisionSettings.verticalWidth * 0.5f, 10.0f, 1.0f),
            -transform.forward, out rayHit, transform.rotation, rayCollisionSettings.back, layerMask))
        {
            transform.position += transform.forward * (rayCollisionSettings.back - rayHit.distance);
        }
        // Right
        if (Physics.BoxCast(transform.position, new Vector3(1.0f, 10.0f, rayCollisionSettings.horizontalWidth * 0.5f),
            transform.right, out rayHit, transform.rotation, rayCollisionSettings.right, layerMask))
        {
            transform.position -= transform.right * (rayCollisionSettings.right - rayHit.distance);
        }
        // Left
        if (Physics.BoxCast(transform.position, new Vector3(1.0f, 10.0f, rayCollisionSettings.horizontalWidth * 0.5f),
            -transform.right, out rayHit, transform.rotation, rayCollisionSettings.left, layerMask))
        {
            transform.position += transform.right * (rayCollisionSettings.left - rayHit.distance);
        }
        boatSplashNoise.SetParameter("Speed", Mathf.Clamp01(linearVelocity.magnitude / MaxSpeed));
    }
}
