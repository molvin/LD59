using TMPro;
using UnityEngine;

public class WalkingControls : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public string ExtraWhenBoated = "Disembark boat by clicking ladders when close to a dock";
    public string ExtraTextWithPolaroid;

    private string commonText;

    void Start()
    {
        commonText = Text.text; 
    }

    void Update()
    {
        string targetText = commonText;
        PolaroidCamera polaroid = GameManager.Get().PolaroidCamera;
        if(polaroid.HasCamera)
        {
            targetText = commonText + "\n" + ExtraTextWithPolaroid;
        }

        Player player = GameManager.Get().Player;
        if(player.Boated)
        {
            targetText += "\n" + ExtraWhenBoated;
        }

        Text.text = targetText;
    }
}
