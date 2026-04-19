using Cysharp.Threading.Tasks.Triggers;
using FMODUnity;
using UnityEngine;

public class OceanSoundMover : MonoBehaviour
{
    public GameObject SoundEmitter;
    public float Radius;

    private void Update()
    {
        Player player = GameManager.Get().Player;

        Vector3 playerPos = player.transform.position;
        playerPos.y = transform.position.y;
        Vector3 point = (playerPos - transform.position).normalized * Radius;
        SoundEmitter.transform.localPosition = point;
        Debug.Log($"Setting sound emitter pos: {point}");
    }
}
