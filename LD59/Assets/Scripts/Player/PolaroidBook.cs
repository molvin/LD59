using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PolaroidBook : MonoBehaviour
{
    public List<Image> Pictures = new List<Image>();
    public GameObject Root;

    public bool IsOpen { get; private set; }

    private List<Texture2D> polaroids = new ();

    private int currentPage;


    private void Start()
    {
        Open(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            PolaroidCamera cam = FindFirstObjectByType<PolaroidCamera>();
            if (cam == null || !cam.TakingPicture)
                Open(!IsOpen);
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
    }

}
