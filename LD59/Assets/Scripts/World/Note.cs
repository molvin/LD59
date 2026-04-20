using TMPro;
using UnityEngine;

public class Note : Pickupable
{
    public TextAsset TextFile;
    public TextMeshProUGUI CanvasText;

    public GameObject CloseUpNote, FarAwayNote;

    private string title;
    private string text;

    public bool InBook;

    private void Start()
    {
        if (TextFile != null)
            UpdateText(TextFile.name, TextFile.text);
    }
    
    private void Update()
    {
        if(holding)
        {
            CloseUpNote.SetActive(true);
            FarAwayNote.SetActive(false);
            CanvasText.text = text;
        }
        else
        {
            CloseUpNote.SetActive(false);
            FarAwayNote.SetActive(true);
            CanvasText.text = "";
        }
    }

    public void UpdateText(string title, string text)
    {
        this.title = title;
        this.text = text;
        CanvasText.text = this.text;

        if(holding)
        {
            CloseUpNote.SetActive(true);
            FarAwayNote.SetActive(false);
            CanvasText.text = text;
        }
        else
        {
            CloseUpNote.SetActive(false);
            FarAwayNote.SetActive(true);
            CanvasText.text = "";
        }
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
