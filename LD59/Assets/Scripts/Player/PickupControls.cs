using TMPro;
using UnityEngine;

public class PickupControls : MonoBehaviour
{
    public string ExtraText = "Pin to Signal Scope: F";
    public string ExtraTextWhenCanDelete = "Destroy Picture: Delete";
    public TextMeshProUGUI Text;

    private string commonText;

    public bool HoldingPolaroid;

    private void Start()
    {
        commonText = Text.text;
    }

    private void Update()
    {
        string targetText = commonText;
        if(HoldingPolaroid)
        {
            targetText += "\n" + ExtraText;
        
            if(GameManager.Get().Book.IsOpen && GameManager.Get().Book.IsInteractingWithPolaroid)
            {
                targetText += "\n" + ExtraTextWhenCanDelete;
            }
        }
        Text.text = targetText;
    }
}
