using TMPro;
using UnityEngine;

public class SignalScope : Interactable
{
    public TextMeshProUGUI Text;
    public Transform AmplitudeDial;
    public Transform FrequencyDial;
    public Renderer SineWaveRenderer;
    public Material SineWaveMat;
    public float MinAmplitude;  
    public float MaxAmplitude;
    public float MinFrequency;
    public float MaxFrequency;

    public float AmplitudeChangeSpeed;
    public float FrequencyChangeSpeed;
    public float MinDialAngle = -135f;
    public float MaxDialAngle = 135f;

    public Transform CameraPoint;
    public float Amplitude;
    public float Frequency;
    public bool Enabled;

    private Player player;
    private Camera cam;
    private Transform camOriginalParent;
    private Vector3 camOriginalLocalPos;
    private Quaternion camOriginalLocalRot;

    protected override void Interact(Transform interactorTransform)
    {
        player = FindFirstObjectByType<Player>();
        player.MovementEnabled = false;

        cam = Camera.main;
        camOriginalParent = cam.transform.parent;
        camOriginalLocalPos = cam.transform.localPosition;
        camOriginalLocalRot = cam.transform.localRotation;

        cam.transform.SetParent(null);
        cam.transform.SetPositionAndRotation(CameraPoint.position, CameraPoint.rotation);

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
    }

    private void Update()
    {
        if(Enabled)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Enabled = false;
                cam.transform.SetParent(camOriginalParent);
                cam.transform.SetLocalPositionAndRotation(camOriginalLocalPos, camOriginalLocalRot);
                player.MovementEnabled = true;
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

            SineWaveMat.SetFloat("_Amplitude", Amplitude);
            SineWaveMat.SetFloat("_Frequency", Frequency);

            float ampT = Mathf.InverseLerp(MinAmplitude, MaxAmplitude, Amplitude);
            float freqT = Mathf.InverseLerp(MinFrequency, MaxFrequency, Frequency);
            float ampAngle = Mathf.Lerp(MinDialAngle, MaxDialAngle, 1 - ampT);
            float freqAngle = Mathf.Lerp(MinDialAngle, MaxDialAngle, 1 - freqT);
            AmplitudeDial.localRotation = Quaternion.Euler(0f, 0f, ampAngle);
            FrequencyDial.localRotation = Quaternion.Euler(0f, 0f, freqAngle);

            Text.text = $"Amp: {Amplitude:F2}, Freq: {Frequency:F2}";
        }
    }
}
