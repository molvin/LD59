using UnityEngine;

public class CircleToNight : CircleAround
{
    protected override void Trigger()
    {
        GameManager.Get().ToggleDayNight();
    }
}
