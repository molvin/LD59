using FMODUnity;
using UnityEngine;

public class JohansFunFmodTalker : MonoBehaviour
{
    public StudioEventEmitter m_SeagulEventEmitter;
    public StudioEventEmitter m_MusicEventEmitter;

    private float m_WhenToPlaySeagul = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_MusicEventEmitter.Play();
        m_WhenToPlaySeagul = Time.time + Random.Range(5f, 10f);
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - m_WhenToPlaySeagul >= 0)
        {
            PlaySeagul();
        }
    }

    private void PlaySeagul()
    {
        m_WhenToPlaySeagul = Time.time + Random.Range(5f, 10f);
        m_SeagulEventEmitter.transform.localPosition = new Vector3(Random.Range(5, 20), 0, Random.Range(5, 20));
        m_SeagulEventEmitter.Play();
    }
}
