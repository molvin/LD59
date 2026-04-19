using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class PolaroidBook : MonoBehaviour
{
    public List<Image> Pictures = new List<Image>();
    public GameObject Root;
    public Button PrevPageButton;
    public Button NextPageButton;
    public Image FocusImage;

    public bool Enabled = true;
    public bool IsOpen { get; private set; }

    private List<Texture2D> polaroids = new();
    private Player player;
    private bool isFocused;

    private int currentPage;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        PrevPageButton.onClick.AddListener(() => FlipPage(-1));
        NextPageButton.onClick.AddListener(() => FlipPage(1));

        FocusImage.gameObject.SetActive(false);

        for (int i = 0; i < Pictures.Count; i++)
        {
            int slot = i;
            EventTrigger trigger = Pictures[i].gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ => OnPictureClicked(slot));
            trigger.triggers.Add(entry);
        }

        Open(false);
    }

    private void Update()
    {
        if (isFocused && Input.GetMouseButtonDown(1))
        {
            Unfocus();
            return;
        }

        if (Enabled && Input.GetKeyDown(KeyCode.G))
        {
            PolaroidCamera cam = FindFirstObjectByType<PolaroidCamera>();
            if (cam == null || !cam.TakingPicture && !player.HoldingPickup)
                Open(!IsOpen);
        }

        if(Enabled && IsOpen && Input.GetMouseButtonDown(1))
            Open(false);
    }

    private void OnPictureClicked(int slot)
    {
        if (isFocused) return;
        int idx = currentPage * Pictures.Count + slot;
        if (idx >= polaroids.Count || polaroids[idx] == null) return;

        isFocused = true;
        var tex = polaroids[idx];
        FocusImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        FocusImage.gameObject.SetActive(true);

        SetBookInteractable(false);
    }

    private void Unfocus()
    {
        isFocused = false;
        FocusImage.gameObject.SetActive(false);
        SetBookInteractable(true);
    }

    private void SetBookInteractable(bool interactable)
    {
        PrevPageButton.interactable = interactable;
        NextPageButton.interactable = interactable;
        foreach (Image img in Pictures)
        {
            img.raycastTarget = interactable;
        }
    }

    public void AddPicture(Texture2D texture)
    {
        polaroids.Add(texture);
        UpdatePictures();
    }

    public void Open(bool open)
    {
        IsOpen = open;
        Root.SetActive(open);
        if (!open && isFocused) Unfocus();
        if (player != null) player.MovementEnabled = !open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;
        UpdatePictures();
    }

    public void FlipPage(int direction)
    {
        int perPage = Pictures.Count;
        int maxPage = perPage > 0 ? Mathf.Max(0, Mathf.CeilToInt((float)polaroids.Count / perPage) - 1) : 0;
        currentPage += direction;
        currentPage = Mathf.Clamp(currentPage, 0, maxPage);
        UpdatePictures();
    }

    private void UpdatePictures()
    {
        int perPage = Pictures.Count;
        int maxPage = perPage > 0 ? Mathf.Max(0, Mathf.CeilToInt((float)polaroids.Count / perPage) - 1) : 0;
        int startIndex = currentPage * perPage;
        for (int i = 0; i < Pictures.Count; i++)
        {
            int idx = startIndex + i;
            if (idx < polaroids.Count && polaroids[idx] != null)
            {
                var tex = polaroids[idx];
                Pictures[i].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                Pictures[i].enabled = true;
            }
            else
            {
                Pictures[i].sprite = null;
                Pictures[i].enabled = false;
            }
        }

        PrevPageButton.gameObject.SetActive(currentPage > 0);
        NextPageButton.gameObject.SetActive(currentPage < maxPage);
    }
}
