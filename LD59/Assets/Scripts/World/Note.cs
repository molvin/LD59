using TMPro;
using UnityEngine;

public class Note : Pickupable
{
    public TextAsset TextFile;
    public TextMeshProUGUI CanvasText;

    private string title;
    private string text;

    public bool InBook;

    private void Start()
    {
        if (TextFile != null)
            UpdateText(TextFile.name, TextFile.text);
    }

    public void UpdateText(string title, string text)
    {
        this.title = title;
        this.text = text;
        CanvasText.text = this.text;
    }

    protected override void Drop()
    {
        if(!InBook)
        {
            FindFirstObjectByType<PolaroidBook>().AddNote(title, text);
            Destroy(gameObject);
        }
        else
        {
            base.Drop();
        }

    }
}
