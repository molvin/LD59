using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PolaroidCamera : MonoBehaviour
{
    public bool Enabled;
    public int Width = 512;
    public int Height = 512;
    public GameObject VisualObject;
    public Transform CameraPoint;
    public Image Output;

    public float PreWaitTime;
    public float TakingPictureWaitTime;
    public float PostWaitTime;

    private bool takingPicture;
    private Vector3 visualObjectStartPos;

    private void Update()
    {
        if(Enabled && Input.GetKeyDown(KeyCode.T) && !takingPicture)
        {
            StartCoroutine(TakePicture());
        }
    }

    private IEnumerator TakePicture()
    {
        // TODO: animation when attached to player
        //       also do something with the output image


        takingPicture = true;
        VisualObject.SetActive(true);
        Output.sprite = null;

        visualObjectStartPos = VisualObject.transform.localPosition;
        for (float t = 0; t < PreWaitTime; t += Time.deltaTime)
        {
            VisualObject.transform.localPosition = Vector3.Lerp(visualObjectStartPos, Vector3.zero, t / PreWaitTime);
            yield return null;
        }
        VisualObject.transform.localPosition = Vector3.zero;

        Transform parent = transform.parent;
        transform.SetParent(null);
        Camera cam = Camera.main;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        var polaroidControlled = FindObjectsByType<PolaroidControlled>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var obj in polaroidControlled)
        {
            obj.gameObject.SetActive(obj.Enabled);
        }

        yield return new WaitForEndOfFrame();

        cam.transform.SetPositionAndRotation(CameraPoint.position, CameraPoint.rotation);
        RenderTexture rt = new(Width, Height, 24);
        cam.targetTexture = rt;
        cam.Render();
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
            obj.gameObject.SetActive(!obj.Enabled);
        }

        yield return new WaitForSeconds(TakingPictureWaitTime);

        for (float t = 0; t < PostWaitTime; t += Time.deltaTime)
        {
            VisualObject.transform.localPosition = Vector3.Lerp(Vector3.zero, visualObjectStartPos, t / PostWaitTime);
            yield return null;
        }
        VisualObject.SetActive(false);
        VisualObject.transform.localPosition = visualObjectStartPos;

        takingPicture = false;
    }
}
