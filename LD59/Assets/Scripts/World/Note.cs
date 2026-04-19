using TMPro;
using UnityEngine;

public class Note : Pickupable
{
    public TextAsset TextFile;
    public TextMeshProUGUI CanvasText;

    private string text;

    private void Start()
    {
        CanvasText.text = TextFile.text;
        text = TextFile.text;
    }

    protected override void Drop()
    {
        FindFirstObjectByType<PolaroidBook>().AddNote(text);
        Destroy(gameObject);
    }
}
