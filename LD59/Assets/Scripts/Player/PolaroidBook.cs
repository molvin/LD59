using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class PolaroidBook : MonoBehaviour
{
    public GameObject Root;

    public bool Enabled = true;
    public bool IsOpen { get; private set; }

    public PolaroidPicture[] pictures;
    public Note[] notes;
    private List<(string, Texture2D)> polaroids = new();
    private List<(string, string)> noteData = new();
    private Player player;
    private int currentPage;
    private bool isInteracting;

    private void Start()
    {
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

                foreach(var n in notes)
                {
                    if (hit.collider.gameObject == n.gameObject)
                    {
                        n.Interact(transform);
                        isInteracting = true;
                        break;
                    }
                }
            }

            if(!isInteracting && Input.GetMouseButtonDown(1))
                Open(false);

            if(isInteracting && Input.GetMouseButtonDown(1))
                isInteracting = false;

            if(!isInteracting && Input.GetKeyDown(KeyCode.A))
            {
                FlipPage(-1);
            }
            if (!isInteracting && Input.GetKeyDown(KeyCode.D))
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

        foreach(var n in notes)
        {
            n.Interactable = open;
        }

        if(open)
            UpdatePicturesAndNotes();
    }

    private int PolaroidPageCount()
    {
        int perPage = pictures.Length;
        return (perPage > 0 && polaroids.Count > 0) ? Mathf.CeilToInt((float)polaroids.Count / perPage) : 0;
    }

    private int NotePageCount()
    {
        int notesPerPage = notes.Length;
        return (notesPerPage > 0 && noteData.Count > 0) ? Mathf.CeilToInt((float)noteData.Count / notesPerPage) : 0;
    }

    public void FlipPage(int direction)
    {
        int totalPages = PolaroidPageCount() + NotePageCount();
        int maxPage = Mathf.Max(0, totalPages - 1);
        currentPage += direction;
        currentPage = Mathf.Clamp(currentPage, 0, maxPage);
        UpdatePicturesAndNotes();
    }

    private void UpdatePicturesAndNotes()
    {
        int perPage = pictures.Length;
        int polaroidPageCount = PolaroidPageCount();
        bool onPolaroidPage = currentPage < polaroidPageCount;

        for (int i = 0; i < pictures.Length; i++)
        {
            PolaroidPicture picture = pictures[i];
            int idx = currentPage * perPage + i;
            if (onPolaroidPage && idx < polaroids.Count)
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

        int notesPerPage = notes.Length;
        int notePage = currentPage - polaroidPageCount;
        for (int i = 0; i < notes.Length; i++)
        {
            Note note = notes[i];
            int idx = notePage * notesPerPage + i;
            if (!onPolaroidPage && notePage >= 0 && idx < noteData.Count)
            {
                (string title, string text) = noteData[idx];
                note.gameObject.SetActive(true);
                note.UpdateText(title, text);
            }
            else
            {
                note.gameObject.SetActive(false);
            }
        }
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
