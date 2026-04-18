using TMPro;
using UnityEngine;

public class SignalScope : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public Transform AmplitudeDial;
    public Transform FrequencyDial;
    public Material SineWaveMat;
    public float MinAmplitude;
    public float MaxAmplitude;
    public float MinFrequency;
    public float MaxFrequency;

    public float AmplitudeChangeSpeed;
    public float FrequencyChangeSpeed;
    public float MinDialAngle = -135f;
    public float MaxDialAngle = 135f;

    public float Amplitude;
    public float Frequency;
    public bool Enabled;

    private void Start()
    {
        SineWaveMat = new Material(SineWaveMat);
        SineWaveMat.SetFloat("_Amplitude", Amplitude);
        SineWaveMat.SetFloat("_Frequency", Frequency);
    }

    private void Update()
    {
        if(Enabled)
        {
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
