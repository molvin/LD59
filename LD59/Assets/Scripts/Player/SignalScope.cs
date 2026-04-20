using Cysharp.Threading.Tasks.Triggers;
using TMPro;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SignalScope : Interactable
{
    [System.Serializable]
    public struct Setting
    {
        public float Amplitude;
        public float Frequency;
        public float Krangle;
    }

    public TextMeshProUGUI Text;
    public Transform AmplitudeDial;
    public Transform FrequencyDial;
    public Transform ModuloDial;
    public Renderer SineWaveRenderer;
    public Material SineWaveMat;
    public float MinAmplitude;  
    public float MaxAmplitude;
    public float MinFrequency;
    public float MaxFrequency;
    public float MinKrangle;
    public float MaxKrangle;

    public float AmplitudeChangeSpeed;
    public float FrequencyChangeSpeed;
    public float KrangleSpeedChange;
    public float MinDialAngle = -135f;
    public float MaxDialAngle = 135f;

    public float MinModulDialAngle = -30f;
    public float MaxModulDialAngle = 30f;

    public Transform CameraPoint;
    public float Amplitude;
    public float Frequency;
    public float Krangle;
    public bool Enabled;

    [Header("Beep")]
    public Renderer LightRenderer;
    public Material LightOnMaterial;
    public Material LightOffMaterial;
    public float BeepMaxInterval = 2f;
    public float BeepMinInterval = 0.1f;
    public EventReference BeepSound;
    public StudioEventEmitter FullBeepSound;

    public float FullBeepThreshold;
    public float VolumeChangeSpeed = 1.0f;

    private float beepTimer;
    private bool lightOn;
    private EventInstance fullBeepInstance;

    public Setting[] CorrectSettings;
    public float CorrectSettingsPercentageMargin = 0.05f;

    private Player player;
    private Camera cam;
    private Transform camOriginalParent;
    private Vector3 camOriginalLocalPos;
    private Quaternion camOriginalLocalRot;
    private PolaroidCamera polaroidCamera;

    public PolaroidPicture PinnedPolaroid;
    public GameObject Tutorial;

    private Texture2D pinnedPicture = null;


    private bool signalScopeMute = false;
    public override void Interact(Transform interactorTransform)
    {
        cam = Camera.main;
        camOriginalParent = cam.transform.parent;
        camOriginalLocalPos = cam.transform.localPosition;
        camOriginalLocalRot = cam.transform.localRotation;

        cam.transform.SetParent(null);
        cam.transform.SetPositionAndRotation(CameraPoint.position, CameraPoint.rotation);

        polaroidCamera = FindFirstObjectByType<PolaroidCamera>();
        if (polaroidCamera != null) polaroidCamera.Enabled = false;

        player = FindFirstObjectByType<Player>();
        player.MovementEnabled = false;
        Enabled = true;
    }

    private void Start()
    {
        SineWaveMat = new Material(SineWaveMat);
        var mats = SineWaveRenderer.materials;
        mats[1] = SineWaveMat;
        SineWaveRenderer.materials = mats;
        SineWaveMat.SetFloat("_Amplitude", Amplitude);
        SineWaveMat.SetFloat("_Frequency", Frequency);
        SineWaveMat.SetFloat("_Krangle", Krangle);
    }

    private void Update()
    {
        if(Enabled)
        {
            cam.transform.SetPositionAndRotation(CameraPoint.position, CameraPoint.rotation);

            if (Input.GetMouseButtonDown(1) && !GameManager.Get().Book.IsOpen)
            {
                Enabled = false;
                cam.transform.SetParent(camOriginalParent);
                cam.transform.SetLocalPositionAndRotation(camOriginalLocalPos, camOriginalLocalRot);
                player.MovementEnabled = true;
                if (polaroidCamera != null) polaroidCamera.Enabled = true;
                return;
            }

            if (Input.GetKey(KeyCode.W))
            {
                Amplitude = Mathf.Clamp(Amplitude + Time.deltaTime * AmplitudeChangeSpeed, MinAmplitude, MaxAmplitude);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Amplitude = Mathf.Clamp(Amplitude - Time.deltaTime * AmplitudeChangeSpeed, MinAmplitude, MaxAmplitude);
            }

            if (Input.GetKey(KeyCode.D))
            {
                Frequency = Mathf.Clamp(Frequency + Time.deltaTime * FrequencyChangeSpeed, MinFrequency, MaxFrequency);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Frequency = Mathf.Clamp(Frequency - Time.deltaTime * FrequencyChangeSpeed, MinFrequency, MaxFrequency);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                Krangle = Mathf.Clamp(Krangle + Time.deltaTime * AmplitudeChangeSpeed, 0, MaxKrangle);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                Krangle = Mathf.Clamp(Krangle - Time.deltaTime * AmplitudeChangeSpeed, 0, MaxKrangle);
            }
            if(Input.GetKey(KeyCode.M))
            {
                if (fullBeepInstance.isValid() && signalScopeMute)
                {
                    signalScopeMute = !signalScopeMute;
                    fullBeepInstance.setParameterByName("ScopeVolume", signalScopeMute ? 0f : 1f);
                }
            }


            float ampT = Mathf.InverseLerp(MinAmplitude, MaxAmplitude, Amplitude);
            float freqT = Mathf.InverseLerp(MinFrequency, MaxFrequency, Frequency);
            float krngT = Mathf.InverseLerp(MinKrangle, MaxKrangle, Krangle);
            float ampAngle = Mathf.Lerp(MinDialAngle, MaxDialAngle, 1 - ampT);
            float freqAngle = Mathf.Lerp(MinDialAngle, MaxDialAngle, 1 - freqT);
            float krangAngle = Mathf.Lerp(MinModulDialAngle, MaxModulDialAngle, 1f - krngT);
            AmplitudeDial.localRotation = Quaternion.Euler(0f, 0f, ampAngle);
            FrequencyDial.localRotation = Quaternion.Euler(0f, 0f, freqAngle);
            ModuloDial.localRotation = Quaternion.Euler(0f, 0f, krangAngle);

            for (int i = 0; i < CorrectSettings.Length; i++)
            {
                var result = GetError(CorrectSettings[i].Amplitude, CorrectSettings[i].Frequency, CorrectSettings[i].Krangle);
                Debug.Log($"Setting {i}: amp error {result.ampError * 100f:F1}%, freq error {result.freqError * 100f:F1}%, krng error {result.krngError * 100f:F1}%, total {result.totalError * 100f:F1}%");
            }
        }

        if (CorrectSettings.Length == WorldManager.Get().Destinations.Count)
        {
            int bestSetting = -1;
            float error = 1;
            for (int i = 0; i < CorrectSettings.Length; i++)
            {
                var result = GetError(CorrectSettings[i].Amplitude, CorrectSettings[i].Frequency, CorrectSettings[i].Krangle);
                if (bestSetting == -1 || result.totalError < error)
                {
                    bestSetting = i;
                    error = result.totalError;
                }
            }

            if (bestSetting >= 0 && error < 0.05f)
            {
                Transform destination = WorldManager.Get().Destinations[bestSetting].transform;
                Transform origin = GameManager.Get().Boat.transform;

                Vector3 toDestination2D = (destination.position - origin.position);
                toDestination2D.y = 0;
                toDestination2D.Normalize();

                float innerProduct = Vector3.Dot(toDestination2D, new Vector3(origin.forward.x, 0, origin.forward.z).normalized);

                float t = (innerProduct + 1.0f) / 2.0f + 0.01f;
                UpdateBeep(GameManager.Get().IsDay ? t : 0);
            }
            else
            {
                UpdateBeep(0);
            }
        }

        SineWaveMat.SetFloat("_Amplitude", Amplitude);
        SineWaveMat.SetFloat("_Frequency", Frequency);
        SineWaveMat.SetFloat("_Krangle", Krangle);
        SineWaveMat.SetColor("_LineColor", GameManager.Get().IsDay ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 1));
        Text.text = "";
        if(!GameManager.Get().IsDay)
        {
            UpdateBeep(0);
        }
    }

    private (float ampError, float freqError, float krngError, float totalError) GetError(float CorrectAmplitude, float CorrectFrequency, float CorrectKrangle)
    {
        float ampRange = MaxAmplitude - MinAmplitude;
        float freqRange = MaxFrequency - MinFrequency;
        float krngRange = MaxKrangle - MinKrangle;
        float ampError = ampRange > 0f ? Mathf.Abs(Amplitude - CorrectAmplitude) / ampRange : 0f;
        float freqError = freqRange > 0f ? Mathf.Abs(Frequency - CorrectFrequency) / freqRange : 0f;
        float krngError = krngRange > 0f ? Mathf.Abs(Krangle - CorrectKrangle) / krngRange : 0f;
        float totalError = (ampError + freqError + krngError) / 3.0f;
        return (ampError, freqError, krngError, totalError);
    }

    private void UpdateBeep(float t)
    {

        if (t > 0)
        {
            Debug.Log($"BEEP: {t}");

            float interval = Mathf.Lerp(BeepMaxInterval, BeepMinInterval, t);
            beepTimer -= Time.deltaTime;
            if (beepTimer <= 0f)
            {
                beepTimer = interval;
                lightOn = !lightOn;
                if (LightRenderer != null)
                    LightRenderer.material = lightOn ? LightOnMaterial : LightOffMaterial;
                if (lightOn && !BeepSound.IsNull && t < FullBeepThreshold)
                {
                    EventInstance beep = RuntimeManager.CreateInstance(BeepSound);
                    beep.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
                    beep.setParameterByName("ScopeVolume", signalScopeMute ? 0f : 1f);
                    beep.start();
                    beep.release();
                }
            }
            if (t >= FullBeepThreshold)
            {
                if (!FullBeepSound.IsPlaying())
                {
                    fullBeepInstance.setParameterByName("ScopeVolume", signalScopeMute ? 0f : 1f);
                    FullBeepSound.Play();
                }
                LightRenderer.material = LightOnMaterial;
            }
            else
            {
                if (FullBeepSound.IsPlaying())
                {
                    FullBeepSound.Stop();
                }
            }
        }
        else
        {
            if (FullBeepSound.IsPlaying())
            {
                FullBeepSound.Stop();
            }

            if (fullBeepInstance.isValid())
            {
                fullBeepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                fullBeepInstance.release();
                fullBeepInstance = default;
            }

            lightOn = false;
            if (LightRenderer != null)
                LightRenderer.material = lightOn ? LightOnMaterial : LightOffMaterial;

        }
    }

    public void PinPolaroid(Texture2D pic)
    {
        pinnedPicture = pic;
        PinnedPolaroid.gameObject.SetActive(true);
        PinnedPolaroid.Picture = pic;
        PinnedPolaroid.Text = "";
        PinnedPolaroid.UpdatePicture();
        Tutorial.SetActive(false);
    }

}
