using TMPro;
using UnityEngine;

public class WalkingControls : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public string ExtraTextWithPolaroid;

    private string commonText;

    void Start()
    {
        commonText = Text.text; 
    }

    void Update()
    {
        PolaroidCamera polaroid = GameManager.Get().PolaroidCamera;
        if(polaroid.HasCamera)
        {
            Text.text = commonText + "\n" + ExtraTextWithPolaroid;
        }
        else
        {
            Text.text = commonText;
        }
    }
}
