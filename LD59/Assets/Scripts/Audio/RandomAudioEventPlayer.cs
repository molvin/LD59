using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;

public class RandomAudioEventPlayer : MonoBehaviour
{
    public EventReference m_Event;
    public float m_Range = 20f;
    public float m_MinInterval = 5f;
    public float m_MaxInterval = 10f;

    private Transform m_AudioChild;
    private StudioEventEmitter m_EventEmitter;
    private float m_WhenToPlay;

    void Start()
    {
        m_AudioChild = new GameObject("SFX Emitter").transform;
        m_EventEmitter = m_AudioChild.GetOrAddComponent<StudioEventEmitter>();
        m_EventEmitter.EventReference = m_Event;
        m_WhenToPlay = Time.time + Random.Range(m_MinInterval, m_MaxInterval);
    }

    void Update()
    {
        if (Time.time >= m_WhenToPlay)
        {
            m_WhenToPlay = Time.time + Random.Range(m_MinInterval, m_MaxInterval);
            m_AudioChild.localPosition = new Vector3(Random.Range(-m_Range, m_Range), 0f, Random.Range(-m_Range, m_Range));
            m_EventEmitter.Play();
        }
    }
}
