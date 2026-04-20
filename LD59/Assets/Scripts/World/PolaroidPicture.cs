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
    private PickupControls controls;

    private void Start()
    {
        UpdatePicture();
        controls = FindFirstObjectByType<PickupControls>(FindObjectsInactive.Include);
    }

    private void Update()
    {
        if(holding)
        {
            controls.HoldingPolaroid = true;

            if (Input.GetKeyDown(KeyCode.F))
            {
                GameManager.Get().SignalScope.PinPolaroid(Picture);
            }
        }
    }

    public void UpdatePicture()
    {
        if(Picture != null)
        {
            CanvasImage.sprite = Sprite.Create(Picture, new Rect(0, 0, Picture.width, Picture.height), new Vector2(0.5f, 0.5f));
            CanvasText.text = Text;
        }
    }

    protected override void Drop()
    {
        controls.HoldingPolaroid = false;
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
