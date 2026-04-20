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
    private List<(string, Texture2D)> polaroids = new();
    private Player player;
    private int currentPage;
    private bool isInteracting;

    private void Start()
    {
        pictures = GetComponentsInChildren<PolaroidPicture>();
        notes = GetComponentsInChildren<Note>();

        player = GameManager.Get().Player;
        Open(false);
    }

    private void Update()
    {
        if (Enabled && !IsOpen && Input.GetKeyDown(KeyCode.G))
        {
            PolaroidCamera cam = GameManager.Get().PolaroidCamera;
            if (cam == null || !cam.TakingPicture && !player.HoldingPickup)
                Open(!IsOpen);
        }

        if(Enabled && IsOpen)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && Input.GetMouseButtonDown(0))
            {
                foreach(var p in pictures)
                {
                    if (hit.collider.gameObject == p.gameObject)
                    {
                        p.Interact(transform);
                        isInteracting = true;
                        break;
                    }
                }
            }

            if(!isInteracting && Input.GetMouseButtonDown(1))
                Open(false);

            if(isInteracting && Input.GetMouseButtonDown(1))
                isInteracting = false;

            if(Input.GetKeyDown(KeyCode.A))
            {
                FlipPage(-1);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                FlipPage(1);
            }
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
            PolaroidPicture picture = pictures[i];

            int idx = startIndex + i;
            if (idx < polaroids.Count)
            {
                (string text, Texture2D texture) = polaroids[idx]; 
                picture.gameObject.SetActive(true);
                picture.Interactable = true;
                picture.Text = text;
                picture.Picture = texture;
                picture.UpdatePicture();
            }
            else
            {
                picture.Interactable = false;
                picture.gameObject.SetActive(false);
            }
        }

    }

    public void AddPicture(string text, Texture2D texture)
    {
        polaroids.Add((text, texture));
    }

    public void AddNote(string title, string text)
    {
        // TODO:
    }
}
