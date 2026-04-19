using UnityEngine;

public class DayNightBound : MonoBehaviour
{
    public bool DayBound = true;
    void Start()
    {
        GameManager.Get().RegisterDayNightBound(this);
    }
}
