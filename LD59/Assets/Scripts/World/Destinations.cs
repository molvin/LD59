using System.Collections.Generic;
using UnityEngine;

public class Destinations : MonoBehaviour
{
    public List<GameObject> Dests = new();

    private void Start()
    {
        WorldManager.Get().Destinations = Dests;
    }
}
