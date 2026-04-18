using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    private new Camera camera;
    private float playerHeight = 1.7f;

    private bool boated = true;
    public Bounds BoatBounds => Utils.GetBounds(GameManager.Get().Boat.gameObject);

    void Start()
    {
        camera = Camera.main;
        if (camera == null)
        {
            camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
        }
        camera.transform.SetParent(transform);
        camera.transform.SetLocalPositionAndRotation(new (0, playerHeight, 0), Quaternion.LookRotation(transform.forward, Vector3.up));
    }

    void Update()
    {
        if (boated)
        {
            Boat boat = GameManager.Get().Boat;
            //boat.Throttle(Input.GetAxisRaw("Vertical"));
            //boat.Steer(Input.GetAxisRaw("Horizontal"));

            transform.position += boat.DeltaVelocity;
            transform.rotation *= Quaternion.AngleAxis(-Mathf.Rad2Deg * boat.DeltaRotation, Vector3.up);

            Bounds boatBounds = BoatBounds;

            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, boatBounds.min.x, boatBounds.max.x);
            clampedPos.y = boatBounds.max.y;
            clampedPos.z = Mathf.Clamp(clampedPos.z, boatBounds.min.z, boatBounds.max.z);

            transform.position = clampedPos;
        }
    }
}
