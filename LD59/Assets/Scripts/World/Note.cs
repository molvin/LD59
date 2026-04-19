using TMPro;
using UnityEngine;

public class Note : Pickupable
{
    public string TextAssetName;
    public TextMeshProUGUI CanvasText;

    private string text;

    private void Start()
    {
        TextAsset txt = Resources.Load<TextAsset>($"Text/{TextAssetName}");
        CanvasText.text = txt.text;
        text = txt.text;
    }

    protected override void Drop()
    {
        FindFirstObjectByType<PolaroidBook>().AddNote(text);
        Destroy(gameObject);
    }
}
