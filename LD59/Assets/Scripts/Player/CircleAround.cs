using FMODUnity;
using UnityEngine;

public abstract class CircleAround : MonoBehaviour
{
    public enum CircleDirection
    {
        Omnidirectional,
        Counterclockwise,
        Clockwise,
    }

    public float MaxDistance = 10.0f;
    public int LapsToActivate = 3;
    public CircleDirection Direction = CircleDirection.Omnidirectional;
    public bool CanRetrigger = false;
    public EventReference TriggerSound;
    public GameObject TriggerAnimation;

    private float? currentProgress;
    private Vector3 lastTrackedDirection;
    protected bool hasTriggered;

    protected abstract void Trigger();

    void Update()
    {
        if (hasTriggered)
        {
            return;
        }

        Player player = GameManager.Get().Player;

        if (Vector3.Distance(player.transform.position, transform.position) > MaxDistance)
        {
            currentProgress = null;
            return;
        }

        if (currentProgress == null)
        {
            currentProgress = float.Epsilon;
            lastTrackedDirection = (player.transform.position - transform.position).normalized;
        }

        Vector3 currentDirection = (player.transform.position - transform.position).normalized;
        float deltaAngle = Vector3.SignedAngle(currentDirection, lastTrackedDirection, Vector3.up);
        currentProgress += deltaAngle;
        lastTrackedDirection = currentDirection;

        bool shouldTrigger = false;
        switch (Direction)
        {
            case CircleDirection.Omnidirectional:
            {
                shouldTrigger = Mathf.Abs((float)currentProgress) >= LapsToActivate * 360.0f;
            } break;
            case CircleDirection.Counterclockwise:
            {
                shouldTrigger = (float)currentProgress >= LapsToActivate * 360.0f;
            } break;
            case CircleDirection.Clockwise:
            {
                shouldTrigger = (float)currentProgress <= LapsToActivate * -360.0f;
            } break;
        }

        if (shouldTrigger)
        {
            if (!TriggerSound.IsNull)
            {
                RuntimeManager.PlayOneShot(TriggerSound, transform.position);
            }
            
            TriggerAnimation.SetActive(true);


            Trigger();
            if (CanRetrigger)
            {
                currentProgress = null;
            }
            else
            {
                hasTriggered = true;
            }
        }
    }
}