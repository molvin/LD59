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
    private List<(string, string)> noteData = new();
    private Player player;
    private int currentPage;
    private bool isInteracting;

    private void Start()
    {
        pictures = GetComponentsInChildren<PolaroidPicture>();
        notes = GetComponentsInChildren<Note>();

        foreach(var p in pictures)
            p.gameObject.SetActive(false);

        foreach(var n in notes)
            n.gameObject.SetActive(false);

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

        UpdatePicturesAndNotes();
    }

    public void FlipPage(int direction)
    {
        int perPage = pictures.Length;
        int maxPage = perPage > 0 ? Mathf.Max(0, Mathf.CeilToInt((float)polaroids.Count / perPage) - 1) : 0;
        currentPage += direction;
        currentPage = Mathf.Clamp(currentPage, 0, maxPage);
        UpdatePicturesAndNotes();
    }

    private void UpdatePicturesAndNotes()
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
        
        // Any notes will appear on the page after the last polaroids, in the same way they can go on forever, but you have to flip through all the polaroid pages to get to the notes

    }

    public void AddPicture(string text, Texture2D texture)
    {
        polaroids.Add((text, texture));
    }

    public void AddNote(string title, string text)
    {
        noteData.Add((title, text));
    }
}
