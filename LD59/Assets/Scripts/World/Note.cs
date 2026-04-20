using TMPro;
using UnityEngine;

public class Note : Pickupable
{
    public TextAsset TextFile;
    public TextMeshProUGUI CanvasText;

    private string title;
    private string text;

    private void Start()
    {
        CanvasText.text = TextFile.text;
        
        title = TextFile.name;
        text = TextFile.text;
    }

    protected override void Drop()
    {
        FindFirstObjectByType<PolaroidBook>().AddNote(title, text);
        Destroy(gameObject);
    }
}
