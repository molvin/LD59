using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PolaroidPicture : Pickupable
{
    public Texture2D Picture;
    public string Text;

    public Image CanvasImage;
    public TextMeshProUGUI CanvasText;

    private void Start()
    {
        CanvasImage.sprite = Sprite.Create(Picture, new Rect(0, 0, Picture.width, Picture.height), new Vector2(0.5f, 0.5f));
        CanvasText.text = Text;
    }

    protected override void Drop()
    {
        FindFirstObjectByType<PolaroidBook>().AddPicture(Picture);    
        Destroy(gameObject);
    }
}
