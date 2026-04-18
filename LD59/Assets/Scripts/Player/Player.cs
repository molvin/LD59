using UnityEngine;

public class Player : MonoBehaviour
{
    private new Camera camera;
    private float playerHeight = 1.7f;

    private bool boated = true;
    public Bounds BoatBounds => Utils.GetBounds(GameManager.Get().Boat);

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
            Bounds boat = BoatBounds;

            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, boat.min.x, boat.max.x);
            clampedPos.y = boat.max.y;
            clampedPos.z = Mathf.Clamp(clampedPos.z, boat.min.z, boat.max.z);

            transform.position = clampedPos;
        }
    }
}
