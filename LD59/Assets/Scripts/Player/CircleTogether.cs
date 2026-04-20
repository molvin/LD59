using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircleTogether : CircleAround
{
    public GameObject ThingToActivate;
    public GameObject LocalThingToActivate;

    protected override void Trigger()
    {
        CanRetrigger = false;
        hasTriggered = true;

        if (LocalThingToActivate != null)
        {
            LocalThingToActivate.SetActive(true);
        }

        CircleTogether[] circleTogethers = FindObjectsByType<CircleTogether>(FindObjectsSortMode.None);
        if (circleTogethers.All(ct => ct.hasTriggered))
        {
            foreach (CircleTogether circle in circleTogethers)
            {
                if (circle.ThingToActivate != null)
                {
                    circle.ThingToActivate.SetActive(true);
                }
            }
        }
    }
}
