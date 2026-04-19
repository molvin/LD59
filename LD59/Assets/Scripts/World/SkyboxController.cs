using UnityEngine;
using static UnityEngine.LightAnchor;

public class SkyboxController : MonoBehaviour
{
    [System.Serializable]
    public struct StarSign
    {
        public Texture2D Texture;
        public float Height;
        public float Size;

        public Transform Target;
    }
    
    [SerializeField] private Material dayTimeSkyboxMaterial;
    [SerializeField] private Material nightTimeSkyboxMaterial;
    [SerializeField] private Light polaroidSun;
    [SerializeField] private Transform polaroidSunTarget;
    [SerializeField] private float polaroidSunHeight = 45.0f;

    [SerializeField] private StarSign[] starSigns;
    [SerializeField] private StarSign moon;

    void Start()
    {
        for (int i = 0; i < starSigns.Length; i++)
        {
            nightTimeSkyboxMaterial.SetTexture("_StarSign" + i + "Tex", starSigns[i].Texture);
        }
    }

    [ContextMenu("Update Skybox")]
    public void UpdateSkybox()
    {
        RenderSettings.skybox = GameManager.Get().IsDay ? dayTimeSkyboxMaterial : nightTimeSkyboxMaterial;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameManager gm = GameManager.Get();
            gm.ToggleDayNight();
        }

        UpdateSkybox();
        UpdatePolaroidSunPosition();
        UpdateStarSigns();
    }

    private Vector3 GetSkyboxDir(Vector3 targetPos, float height)
    {
        Player player = GameManager.Get().Player;
        Vector3 planarDirToTarget = (targetPos - player.transform.position).normalized;
        planarDirToTarget.y = 0.0f;

        float elevationRad = height * Mathf.Deg2Rad;
        return planarDirToTarget * Mathf.Cos(elevationRad) + Vector3.up * Mathf.Sin(elevationRad);
    }

    private void UpdatePolaroidSunPosition()
    {
        polaroidSun.transform.forward = -GetSkyboxDir(polaroidSunTarget.position, polaroidSunHeight);
    }

    private void UpdateStarSigns()
    {
        Player player = GameManager.Get().Player;
        for (int i = 0; i < starSigns.Length; i++)
        {
            StarSign sign = starSigns[i];
            string name = $"_StarSign{i}";
            nightTimeSkyboxMaterial.SetVector(name + "Dir", GetSkyboxDir(sign.Target.position, sign.Height));
            nightTimeSkyboxMaterial.SetFloat(name + "Size", sign.Size);
        }

        nightTimeSkyboxMaterial.SetVector("_MoonDir", GetSkyboxDir(moon.Target.position, moon.Height));
        nightTimeSkyboxMaterial.SetFloat("_MoonSize", moon.Size);
    }
}
