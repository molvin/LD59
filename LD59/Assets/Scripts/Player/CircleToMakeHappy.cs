using UnityEngine;

public class CircleToMakeHappy : CircleAround
{
    protected override void Trigger()
    {
        GameManager.Get().RegisterHappyPillar(gameObject);
    }
}
