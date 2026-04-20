using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PolaroidPicture : Pickupable
{
    public Texture2D Picture;
    public string Text;

    public Image CanvasImage;
    public TextMeshProUGUI CanvasText;

    public bool InBook;

    private void Start()
    {
        UpdatePicture();
    }

    public void UpdatePicture()
    {
        CanvasImage.sprite = Sprite.Create(Picture, new Rect(0, 0, Picture.width, Picture.height), new Vector2(0.5f, 0.5f));
        CanvasText.text = Text;
    }

    protected override void Drop()
    {
        if(InBook)
        {
            base.Drop();
        }
        else
        {
            GameManager.Get().Book.AddPicture(Text, Picture);
            Destroy(gameObject);
        }
    }
}
