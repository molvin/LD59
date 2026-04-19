using FMODUnity;
using UnityEngine;

public class Boat : MonoBehaviour
{
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

    private float throttle = 0.0f;
    private float steering = 0.0f;

    private Vector2 linearVelocity;
    private float angularVelocity;
    private Vector3 deltaVelocity;
    private float deltaRotation;
    private bool isSteering = false;

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
    }

    public void Steer(float input)
    {
        float turnJerk = input * WheelSpeed * Time.deltaTime;
        steering = Mathf.Clamp(steering + turnJerk, -1.0f, 1.0f);

        isSteering = true;
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 currentPlaneForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        if (!isSteering)
        {
            steering *= Mathf.Pow(WheelReset, Time.deltaTime);
        }
        isSteering = false;
        Wheel.transform.localRotation = Quaternion.Euler(0, 0, -300 * steering);
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
    }
}
