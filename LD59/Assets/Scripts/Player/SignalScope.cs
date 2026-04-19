using TMPro;
using UnityEngine;

public class SignalScope : Interactable
{
    [System.Serializable]
    public struct Setting
    {
        public float Amplitude;
        public float Frequency;
    }

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

    public Setting[] CorrectSettings;
    public float CorrectSettingsPercentageMargin = 0.05f;

    private Player player;
    private Camera cam;
    private Transform camOriginalParent;
    private Vector3 camOriginalLocalPos;
    private Quaternion camOriginalLocalRot;
    private PolaroidCamera polaroidCamera;
    private PolaroidBook polaroidBook;

    protected override void Interact(Transform interactorTransform)
    {
        cam = Camera.main;
        camOriginalParent = cam.transform.parent;
        camOriginalLocalPos = cam.transform.localPosition;
        camOriginalLocalRot = cam.transform.localRotation;

        cam.transform.SetParent(null);
        cam.transform.SetPositionAndRotation(CameraPoint.position, CameraPoint.rotation);

        polaroidCamera = FindFirstObjectByType<PolaroidCamera>();
        if (polaroidCamera != null) polaroidCamera.Enabled = false;

        polaroidBook = FindFirstObjectByType<PolaroidBook>();
        if (polaroidBook != null)
        {
            polaroidBook.Open(false);
            polaroidBook.Enabled = false;
        }
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
    }

    private void Update()
    {
        if(Enabled)
        {
            cam.transform.SetPositionAndRotation(CameraPoint.position, CameraPoint.rotation);

            if (Input.GetMouseButtonDown(1))
            {
                Enabled = false;
                cam.transform.SetParent(camOriginalParent);
                cam.transform.SetLocalPositionAndRotation(camOriginalLocalPos, camOriginalLocalRot);
                player.MovementEnabled = true;
                if (polaroidCamera != null) polaroidCamera.Enabled = true;
                if (polaroidBook != null) polaroidBook.Enabled = true;
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

            for (int i = 0; i < CorrectSettings.Length; i++)
            {
                var result = GetError(CorrectSettings[i].Amplitude, CorrectSettings[i].Frequency);
                Debug.Log($"Setting {i}: amp error {result.ampError * 100f:F1}%, freq error {result.freqError * 100f:F1}%, total {result.totalError * 100f:F1}%");
            }
        }

        if (CorrectSettings.Length == WorldManager.Get().Destinations.Count)
        {
            int bestSetting = -1;
            float error = 1;
            for (int i = 0; i < CorrectSettings.Length; i++)
            {
                var result = GetError(CorrectSettings[i].Amplitude, CorrectSettings[i].Frequency);
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

                float innerProduct = Vector3.Dot((destination.position - origin.position).normalized, origin.forward);
                Debug.Log($"BEEP: {innerProduct}");
            }
        }
    }

    private (float ampError, float freqError, float totalError) GetError(float CorrectAmplitude, float CorrectFrequency)
    {
        float ampRange = MaxAmplitude - MinAmplitude;
        float freqRange = MaxFrequency - MinFrequency;
        float ampError = ampRange > 0f ? Mathf.Abs(Amplitude - CorrectAmplitude) / ampRange : 0f;
        float freqError = freqRange > 0f ? Mathf.Abs(Frequency - CorrectFrequency) / freqRange : 0f;
        float totalError = (ampError + freqError) * 0.5f;
        return (ampError, freqError, totalError);
    }
}
