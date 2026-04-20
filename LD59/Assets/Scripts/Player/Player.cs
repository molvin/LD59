using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    [System.Serializable]
    public struct FootSound
    {
        public string name;
        public int sound;
    }
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
    public EventReference Footstep;
    public List<FootSound> FootstepSounds;
    public float FootstepStride = 0.8f;
    private float distanceTraveledSinceLastFootstep;

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
    private string textureName = "";
    private bool isDay = true;

    public EventReference NightSwitchSound;
    public Animator CameraAnimator;

    private bool inStartAnimation;

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
        StartAnimation();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CameraAnimator.enabled = false;
    }

    public void Unstuck()
    {
        transform.position = GameManager.Get().Boat.transform.position + Vector3.up * 0.5f;
        Reset();
    }

    public string GetTerrainTextureName(Vector3 hitPoint, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int mapX = (int)(((hitPoint.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)(((hitPoint.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float maxWeight = 0;
        int dominantIndex = 0;

        for (int i = 0; i < splatmapData.GetLength(2); i++)
        {
            if (splatmapData[0, 0, i] > maxWeight)
            {
                maxWeight = splatmapData[0, 0, i];
                dominantIndex = i;
            }
        }

        return terrainData.terrainLayers[dominantIndex].name;
    }

    void Update()
    {
        CheckForDayNightShift();
        
        if (Input.GetKeyDown(KeyCode.K) && Time.time - lastTimeStandingOnBoat > 120.0f)
        {
            Unstuck();
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

            Terrain terrain = rayHit.collider.GetComponent<Terrain>();
            Renderer renderer = rayHit.collider.GetComponent<Renderer>();

            if (terrain != null)
            {
                textureName = GetTerrainTextureName(rayHit.point, terrain);
            }
            else if (renderer != null)
            {
                textureName = renderer.material.name;
            }
        }

        if (MovementEnabled)
        {
            float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

            if(!inStartAnimation)
            {
                rotation -= mouseY;
                rotation = Mathf.Clamp(rotation, -90f, 90f);

                camera.transform.localRotation = Quaternion.Euler(rotation, 0f, 0f);
                transform.Rotate(Vector3.up * mouseX);
            }


            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1.0f);
            if (inStartAnimation)
                input = Vector3.zero;

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
                    //velocity -= dot * normal;
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

        Vector3 lastOffset = localSpaceOffset;
        localSpaceOffset = boat.transform.InverseTransformPoint(transform.position);

        distanceTraveledSinceLastFootstep += Vector3.Distance(lastOffset, localSpaceOffset);
        if (distanceTraveledSinceLastFootstep > FootstepStride)
        {
            distanceTraveledSinceLastFootstep = 0;

            int terrainSound = 0;
            if (FootstepSounds.Any(foot => foot.name.Equals(textureName)))
            {
                terrainSound = FootstepSounds.Find(foot => foot.name.Equals(textureName)).sound;
            }

            if (!Footstep.IsNull)
            {
                EventInstance step = RuntimeManager.CreateInstance(Footstep);
                step.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
                step.setParameterByName("Terrain", terrainSound);
                step.start();
                step.release();
            }
        }

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
    
    private void CheckForDayNightShift()
    {
        IEnumerator DayShiftAnimation(bool goingToNight)
        {
            CameraAnimator.enabled = true;
            MovementEnabled = false;
            RuntimeManager.PlayOneShot(NightSwitchSound, transform.position);

            CameraAnimator.SetTrigger("FallDown");

            yield return new WaitForSeconds(1.0f);

            float duration = 4.0f;
            float t = 0.0f;
            SkyboxController skybox = FindFirstObjectByType<SkyboxController>();
            skybox.UpdateSkybox();
            while (t < duration)
            {
                skybox.SetDayNightTransition(t / duration);
                t += Time.deltaTime;
                yield return null;
            }
            skybox.SetDayNightTransition(1);

                yield return new WaitForSeconds(1.0f);

            MovementEnabled = true;
            CameraAnimator.enabled = false;
            rotation = 0.0f;
        }

        if (GameManager.Get().IsDay != isDay)
        {
            bool goingToNight = isDay;
            isDay = GameManager.Get().IsDay;
            StartCoroutine(DayShiftAnimation(goingToNight));
        }
    }

    private void StartAnimation()
    {
        IEnumerator Animation()
        {
            inStartAnimation = true;
            Transform sp = GameManager.Get().Boat.StartPosition;

            float duration = 0.5f;
            float t = 0.0f;
            Vector3 forward = sp.transform.forward;
            forward.y = 0;
            transform.forward = forward;
            camera.transform.SetLocalPositionAndRotation(new(0, PLAYER_HEIGHT, 0), Quaternion.identity);

            while (t < duration)
            {
                transform.position = sp.transform.position;
                localSpaceOffset = transform.position - GameManager.Get().Boat.transform.position;
                t += Time.deltaTime;
                yield return null;
            }

            inStartAnimation = false;
        }
        StartCoroutine(Animation());
    }
}
