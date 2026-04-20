using TMPro;
using UnityEngine;

public class PickupControls : MonoBehaviour
{
    public string ExtraText = "Pin to Signal Scope: F";
    public TextMeshProUGUI Text;

    private string commonText;

    public bool HoldingPolaroid;

    private void Start()
    {
        commonText = Text.text;
    }

    private void Update()
    {
        if(HoldingPolaroid)
        {
            Text.text = commonText + "\n" + ExtraText;
        }
        else
        {
            Text.text = commonText;
        }
    }
}
