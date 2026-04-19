using UnityEngine;
using UnityEngine.UI;

public class PolaroidPicture : Pickupable
{
    public Texture2D Picture;
    public Image CanvasImage;

    private void Start()
    {
        CanvasImage.sprite = Sprite.Create(Picture, new Rect(0, 0, Picture.width, Picture.height), new Vector2(0.5f, 0.5f));
    }

    protected override void Drop()
    {
        FindFirstObjectByType<PolaroidBook>().AddPicture(Picture);    
        Destroy(gameObject);
    }
}
