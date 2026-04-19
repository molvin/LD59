using TMPro;
using UnityEngine;

public class Note : Pickupable
{
    public string Text;
    public TextMeshProUGUI CanvasText;

    private void Start()
    {
        CanvasText.text = Text;
    }

    protected override void Drop()
    {
        FindFirstObjectByType<PolaroidBook>().AddNote(Text);
        Destroy(gameObject);
    }
}
