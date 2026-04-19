using UnityEngine;

public class Floater : MonoBehaviour
{
    public Material WaterMaterial;
    public float WaterBaseY = 0f;
    public float TiltStrength = 1f;

    private static readonly int AmpId   = Shader.PropertyToID("_WaveAmplitude");
    private static readonly int FreqId  = Shader.PropertyToID("_WaveFrequency");
    private static readonly int SpeedId = Shader.PropertyToID("_WaveSpeed");

    private float WaveHeight(float x, float z, float amp, float freq, float speed)
    {
        float t = Time.time * speed;
        float f = freq;
        float h  = Mathf.Sin(x * f + t);
               h += Mathf.Sin((x * 0.8f + z * 0.6f) * f * 1.3f + t * 0.9f) * 0.6f;
               h += Mathf.Sin(z * f * 0.7f - t * 1.1f) * 0.4f;
        return h * amp;
    }

    private Vector3 originXZ;

    private void Start()
    {
        originXZ = transform.position;
    }

    private void Update()
    {
        if (WaterMaterial == null) return;

        float amp   = WaterMaterial.GetFloat(AmpId);
        float freq  = WaterMaterial.GetFloat(FreqId);
        float speed = WaterMaterial.GetFloat(SpeedId);

        float x = originXZ.x;
        float z = originXZ.z;

        const float delta = 0.1f;
        float y  = WaterBaseY + WaveHeight(x, z, amp, freq, speed);
        float yX = WaterBaseY + WaveHeight(x + delta, z, amp, freq, speed);
        float yZ = WaterBaseY + WaveHeight(x, z + delta, amp, freq, speed);

        Vector3 tangentX = new(delta, yX - y, 0f);
        Vector3 tangentZ = new(0f, yZ - y, delta);
        Vector3 normal   = Vector3.Cross(tangentZ, tangentX).normalized;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0f, transform.eulerAngles.y, 0f),
            TiltStrength * Time.deltaTime
        );
    }
}
