using UnityEngine;

public class CircleToMakeHappy : CircleAround
{
    public int Index;

    protected override void Trigger()
    {
        GameManager.Get().RegisterHappyPillar(Index);
    }
}
