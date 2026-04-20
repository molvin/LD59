using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PolaroidCamera : MonoBehaviour
{
    public bool HasCamera;
    public bool Enabled;
    public int Width = 512;
    public int Height = 512;
    public GameObject VisualObject;
    public Transform CameraPoint;
    public Image Output;
    public GameObject PolaroidPicture;

    public float PreWaitTime;
    public float IntermediateWaitTime;
    public float TakingPictureWaitTime;
    public float PostWaitTime;

    public Animator Anim;
    public List<Renderer> IgnoredRenderers = new();
    public EventReference SnapSound;
    public EventReference WhirrSound;

    public bool TakingPicture { get; private set; }
    private Vector3 visualObjectStartPos;

    private void Update()
    {
        PolaroidBook book = FindFirstObjectByType<PolaroidBook>();
        if(HasCamera && Enabled && Input.GetKeyDown(KeyCode.T) && !TakingPicture && (book == null || !book.IsOpen) && !GameManager.Get().Player.HoldingPickup)
        {
            StartCoroutine(TakePicture());
        }
    }

    private IEnumerator TakePicture()
    {
        Player player = GameManager.Get().Player;

        TakingPicture = true;
        VisualObject.SetActive(true);
        Output.sprite = null;
        PolaroidPicture.SetActive(false);

        visualObjectStartPos = VisualObject.transform.localPosition;
        
        Anim.SetTrigger("MakeReady");
        yield return new WaitForSeconds(PreWaitTime);
        VisualObject.transform.localPosition = Vector3.zero;
        yield return new WaitForEndOfFrame();

        player.MovementEnabled = false;

        Transform parent = transform.parent;
        transform.SetParent(null);
        Camera cam = Camera.main;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        var polaroidControlled = FindObjectsByType<PolaroidControlled>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var obj in polaroidControlled)
        {
            bool isDay = GameManager.Get().IsDay;
            bool ignore = !isDay && obj.IgnoreIfNightTime;
            if(!ignore) 
                obj.gameObject.SetActive(obj.ShowInPhoto);
        }

        SkyboxController skybox = FindFirstObjectByType<SkyboxController>(); 

        // cam.transform.SetPositionAndRotation(CameraPoint.position, CameraPoint.rotation);
        foreach (var r in IgnoredRenderers) r.enabled = false;

        RuntimeManager.PlayOneShot(SnapSound, transform.position);

        skybox.TakePicture();
        RenderTexture rt = new(Width, Height, 24);
        cam.targetTexture = rt;
        cam.Render();
        foreach (var r in IgnoredRenderers) r.enabled = true;
        skybox.EndPicture();
        cam.transform.position = startPos;
        cam.transform.rotation = startRot;

        RenderTexture.active = rt;
        Texture2D photo = new(Width, Height, TextureFormat.RGB24, false);
        photo.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        photo.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        Sprite sprite = Sprite.Create(photo, new Rect(0, 0, Width, Height), new Vector2(0.5f, 0.5f));
        if (Output.sprite != null)
            Destroy(Output.sprite.texture);
        Output.sprite = sprite;



        transform.parent = parent;

        foreach(var obj in polaroidControlled)
        {
            bool isDay = GameManager.Get().IsDay;
            bool ignore = !isDay && obj.IgnoreIfNightTime;
            if(!ignore) 
                obj.gameObject.SetActive(!obj.ShowInPhoto);     
       }

        yield return new WaitForSeconds(TakingPictureWaitTime);

        Anim.SetTrigger("LookAt");
        RuntimeManager.PlayOneShot(WhirrSound, transform.position);

        yield return new WaitForSeconds(IntermediateWaitTime);
        PolaroidPicture.SetActive(true);

        while (true)
        {
            if(Input.GetKeyDown(KeyCode.D))
            {
                PolaroidBook book = FindFirstObjectByType<PolaroidBook>();
                if (book != null)
                    book.AddPicture("Temp", photo);
                break;
            }
            else if(Input.GetKeyDown(KeyCode.A)) 
            {
                break;
            }

            yield return null;
        }
        PolaroidPicture.SetActive(false);

        Anim.SetTrigger("PutAway");
        yield return new WaitForSeconds(PostWaitTime);
        VisualObject.SetActive(false);
        VisualObject.transform.localPosition = visualObjectStartPos;

        player.MovementEnabled = true;
        TakingPicture = false;
    }
}
