using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PolaroidCamera : MonoBehaviour
{
    public bool Enabled;
    public int Width = 512;
    public int Height = 512;
    public Transform CameraPoint;
    public Image Output;

    private bool takingPicture;

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

        yield return new WaitForEndOfFrame();

        Camera cam = Camera.main;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        cam.transform.position = CameraPoint.position;
        cam.transform.rotation = CameraPoint.rotation;
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

        takingPicture = false;
    }
}
