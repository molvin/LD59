using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Centroid : MonoBehaviour
{
    public Image Image;

    public Sprite Default;
    public Sprite Interactable;

    public float DefaultSize = 5;
    public float InteractableSize = 15;

    private void Update()
    {
        GameManager gm = GameManager.Get();
        bool shouldBeEnabled = !gm.Player.isSteering && !gm.PolaroidCamera.TakingPicture && !gm.Book.IsOpen && !gm.Player.HoldingPickup && !gm.SignalScope.Enabled;
        Image.enabled = shouldBeEnabled;

        if(shouldBeEnabled)
        {
            Transform camT = Camera.main.transform;
            var interactables = FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            RectTransform rectTransform = GetComponent<RectTransform>();
            bool lookingAtInteractable = interactables.Any(x => x.CanInteract(camT));
            Image.sprite = !lookingAtInteractable ? Default : Interactable;

            float size = !lookingAtInteractable ? DefaultSize : InteractableSize;
            rectTransform.sizeDelta = new Vector2(size, size);
        }
    }
}
