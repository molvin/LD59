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
    [System.Serializable]
    public struct Moon
    {
        public Texture2D[] Phases;
        public float Height;
        public float Size;

        public Transform Target;
    }


    [SerializeField] private Material dayTimeSkyboxMaterial;
    [SerializeField] private Material nightTimeSkyboxMaterial;
    [SerializeField] private Material polaroidNightTimeSkyboxMaterial;
    [SerializeField] private Light polaroidSun;
    [SerializeField] private Light daySun;
    [SerializeField] private Light nightSun;
    [SerializeField] private Transform polaroidSunTarget;
    [SerializeField] private float polaroidSunHeight = 45.0f;

    [SerializeField] private StarSign[] starSigns;
    [SerializeField] private Moon moon;

    [Header("Horizon")]
    [SerializeField] private float PlayerMinHeight = 0.0f;
    [SerializeField] private float PlayerMaxHeight = 18.0f;
    [SerializeField] private float MinHorizonOffset = 0.09f;
    [SerializeField] private float MaxHorizonOffset = 0.27f;

    private int circledPillars = 0;

    private void Start()
    {
        dayTimeSkyboxMaterial = new Material(dayTimeSkyboxMaterial);
        polaroidNightTimeSkyboxMaterial = new Material(polaroidNightTimeSkyboxMaterial);
        nightTimeSkyboxMaterial = new Material(nightTimeSkyboxMaterial);

        for (int i = 0; i < starSigns.Length; i++)
        {
            polaroidNightTimeSkyboxMaterial.SetTexture("_StarSign" + i + "Tex", starSigns[i].Texture);
        }
        polaroidNightTimeSkyboxMaterial.SetTexture("_MoonTex", null);
        UpdateSkybox();
    }

    [ContextMenu("Update Skybox")]
    public void UpdateSkybox()
    {
        bool isDay = GameManager.Get().IsDay;
        RenderSettings.skybox = isDay ? dayTimeSkyboxMaterial : nightTimeSkyboxMaterial;
        nightSun.gameObject.SetActive(!isDay);
        daySun.gameObject.SetActive(isDay);
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
        UpdateHorizonDropoff();
        UpdateMoonPhase();
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
            polaroidNightTimeSkyboxMaterial.SetVector(name + "Dir", GetSkyboxDir(sign.Target.position, sign.Height));
            polaroidNightTimeSkyboxMaterial.SetFloat(name + "Size", sign.Size);
        }

        polaroidNightTimeSkyboxMaterial.SetVector("_MoonDir", GetSkyboxDir(moon.Target.position, moon.Height));
        polaroidNightTimeSkyboxMaterial.SetFloat("_MoonSize", moon.Size);
    }

    public void TakePicture()
    {
        if(!GameManager.Get().IsDay)
        {
            RenderSettings.skybox = polaroidNightTimeSkyboxMaterial;
        }    
        else
        {
            RenderSettings.skybox = dayTimeSkyboxMaterial;
        }
    }

    public void EndPicture()
    {
        if(!GameManager.Get().IsDay)
        {
            RenderSettings.skybox = nightTimeSkyboxMaterial;
        }
        else
        {
            RenderSettings.skybox = dayTimeSkyboxMaterial;
        }
    }

    private void UpdateHorizonDropoff()
    {
        Player player = GameManager.Get().Player;
        float t = Mathf.InverseLerp(PlayerMinHeight, PlayerMaxHeight, player.transform.position.y);
        float horizonOffset = Mathf.Lerp(MinHorizonOffset, MaxHorizonOffset, t);
        dayTimeSkyboxMaterial.SetFloat("_HorizonOffset", horizonOffset);
        nightTimeSkyboxMaterial.SetFloat("_HorizonOffset", horizonOffset);
        polaroidNightTimeSkyboxMaterial.SetFloat("_HorizonOffset", horizonOffset);
    }

    private void UpdateMoonPhase()
    {
        int x = GameManager.Get().HappyPillarCount;
        if (x != circledPillars)
        {
            circledPillars = x;
            polaroidNightTimeSkyboxMaterial.SetTexture("_MoonTex", moon.Phases[circledPillars]);
        }
    }
}
