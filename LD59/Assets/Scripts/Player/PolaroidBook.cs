using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class PolaroidBook : MonoBehaviour
{

    public GameObject Root;

    public bool Enabled = true;
    public bool IsOpen { get; private set; }


    private PolaroidPicture[] pictures;
    private Note[] notes;

    private List<Texture2D> polaroids = new();
    private Player player;
    private int currentPage;

    private void Start()
    {
        pictures = GetComponentsInChildren<PolaroidPicture>();
        notes = GetComponentsInChildren<Note>();

        player = GameManager.Get().Player;
        Open(false);
    }

    private void Update()
    {
        if (Enabled && Input.GetKeyDown(KeyCode.G))
        {
            PolaroidCamera cam = GameManager.Get().PolaroidCamera;
            if (cam == null || !cam.TakingPicture && !player.HoldingPickup)
                Open(!IsOpen);
        }

        if(Enabled && IsOpen)
        {
            foreach(var p in pictures)
            {
                p.TryInteract(Camera.main.transform);
            }

            if(Input.GetMouseButtonDown(1))
                Open(false);
        }
    }

    public void Open(bool open)
    {
        IsOpen = open;
        Root.SetActive(open);
        player.MovementEnabled = !open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;

        foreach(var p in pictures)
        {
            p.Interactable = open;
        }

        UpdatePictures();
    }

    public void FlipPage(int direction)
    {
        int perPage = pictures.Length;
        int maxPage = perPage > 0 ? Mathf.Max(0, Mathf.CeilToInt((float)polaroids.Count / perPage) - 1) : 0;
        currentPage += direction;
        currentPage = Mathf.Clamp(currentPage, 0, maxPage);
        UpdatePictures();
    }

    private void UpdatePictures()
    {
        int perPage = pictures.Length;
        int maxPage = perPage > 0 ? Mathf.Max(0, Mathf.CeilToInt((float)polaroids.Count / perPage) - 1) : 0;
        int startIndex = currentPage * perPage;

        for (int i = 0; i < pictures.Length; i++)
        {
            int idx = startIndex + i;
            if (idx < polaroids.Count && polaroids[idx] != null)
            {
                pictures[i].gameObject.SetActive(true);
            }
            else
            {
                pictures[i].gameObject.SetActive(false);
            }
        }

    }

    public void AddPicture(Texture2D texture)
    {
        polaroids.Add(texture);
    }

    public void AddNote(string title, string text)
    {
        // TODO:
    }
}
