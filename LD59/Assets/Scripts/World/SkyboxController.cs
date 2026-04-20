using System.Collections;
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
    [SerializeField] private Light polaroidSun;
    [SerializeField] private Light daySun;
    [SerializeField] private Light nightSun;
    [SerializeField] private Transform polaroidSunTarget;
    [SerializeField] private float polaroidSunHeight = 45.0f;

    [SerializeField] private StarSign[] starSigns;
    [SerializeField] private Moon moon;
    [SerializeField] private float MoonMoveDuration = 8.0f;

    [Header("Horizon")]
    [SerializeField] private float PlayerMinHeight = 0.0f;
    [SerializeField] private float PlayerMaxHeight = 18.0f;
    [SerializeField] private float MinHorizonOffset = 0.09f;
    [SerializeField] private float MaxHorizonOffset = 0.27f;

    private int circledPillars = 0;
    private Vector3 moonDir = Vector3.up;

    private float dayIntensity;
    private float nightIntensity;
    private float dayTemp;
    private float nightTemp;
    private Color dayColor;
    private Color nightColor;

    private void Start()
    {
        dayIntensity = daySun.intensity;
        dayTemp = daySun.colorTemperature;
        nightIntensity = nightSun.intensity;
        nightTemp = nightSun.colorTemperature;
        dayColor = daySun.color;
        nightColor = nightSun.color;

        dayTimeSkyboxMaterial = new Material(dayTimeSkyboxMaterial);
        nightTimeSkyboxMaterial = new Material(nightTimeSkyboxMaterial);

        for (int i = 0; i < starSigns.Length; i++)
        {
            nightTimeSkyboxMaterial.SetTexture("_StarSign" + i + "Tex", starSigns[i].Texture);
        }
        nightTimeSkyboxMaterial.SetTexture("_MoonTex", moon.Phases[0]);
        UpdateSkybox();
        UpdateStarSigns(true);
    }

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
        if(Input.GetKeyDown(KeyCode.L))
        {
            GameManager.Get().happyPillars.Add(GameManager.Get().HappyPillarCount);
        }

        // UpdateSkybox();
        UpdatePolaroidSunPosition();
        UpdateNightSunPosition();
        UpdateStarSigns(false);
        UpdateHorizonDropoff();
        UpdateMoonPhase();
    }

    private Vector3 GetSkyboxDir(Vector3 targetPos, float height)
    {
        Player player = GameManager.Get().Player;
        targetPos.y = player.transform.position.y;
        Vector3 planarDirToTarget = (targetPos - player.transform.position).normalized;
        planarDirToTarget.y = 0.0f;

        float elevationRad = height * Mathf.Deg2Rad;
        return planarDirToTarget * Mathf.Cos(elevationRad) + Vector3.up * Mathf.Sin(elevationRad);
    }

    private void UpdatePolaroidSunPosition()
    {
        polaroidSun.transform.forward = -GetSkyboxDir(polaroidSunTarget.position, polaroidSunHeight);
    }

    private void UpdateStarSigns(bool updateSize)
    {
        Player player = GameManager.Get().Player;
        for (int i = 0; i < starSigns.Length; i++)
        {
            StarSign sign = starSigns[i];
            string name = $"_StarSign{i}";
            nightTimeSkyboxMaterial.SetVector(name + "Dir", GetSkyboxDir(sign.Target.position, sign.Height));
            if(updateSize)
                nightTimeSkyboxMaterial.SetFloat(name + "Size", sign.Size);
        }

        nightTimeSkyboxMaterial.SetVector("_MoonDir", moonDir);
        nightTimeSkyboxMaterial.SetFloat("_MoonSize", moon.Size);
    }

    public void TakePicture()
    {
        /*
        if(!GameManager.Get().IsDay)
        {
            RenderSettings.skybox = polaroidNightTimeSkyboxMaterial;
        }    
        else
        {
            RenderSettings.skybox = dayTimeSkyboxMaterial;
        }
        */
    }

    public void EndPicture()
    {
        /*
        if(!GameManager.Get().IsDay)
        {
            RenderSettings.skybox = nightTimeSkyboxMaterial;
        }
        else
        {
            RenderSettings.skybox = dayTimeSkyboxMaterial;
        }
        */
    }

    private void UpdateHorizonDropoff()
    {
        Player player = GameManager.Get().Player;
        float t = Mathf.InverseLerp(PlayerMinHeight, PlayerMaxHeight, player.transform.position.y);
        float horizonOffset = Mathf.Lerp(MinHorizonOffset, MaxHorizonOffset, t);
        dayTimeSkyboxMaterial.SetFloat("_HorizonOffset", horizonOffset);
        nightTimeSkyboxMaterial.SetFloat("_HorizonOffset", horizonOffset);
    }

    private void UpdateNightSunPosition()
    {
        nightSun.transform.forward = -moonDir;
    }

    private void UpdateMoonPhase()
    {
        int x = GameManager.Get().HappyPillarCount;
        if (x != circledPillars)
        {
            circledPillars = x;
            nightTimeSkyboxMaterial.SetTexture("_MoonTex", moon.Phases[circledPillars]);

            foreach(int idx in GameManager.Get().happyPillars)
            {
                if (idx == 0)
                    nightTimeSkyboxMaterial.SetFloat("_StarSign0Size", 0);
                if (idx == 1)
                    nightTimeSkyboxMaterial.SetFloat("_StarSign1Size", 0);
                if (idx == 2)
                    nightTimeSkyboxMaterial.SetFloat("_StarSign2Size", 0);
            }


            // TODO: when all pillars have been circled, slowly move the moon into position
            if (circledPillars == 3)
            {
                StartCoroutine(MoveMoon());
            }
        }
    }
    private IEnumerator MoveMoon()
    {
        Vector3 currentDir = moonDir;
        Vector3 targetDir = GetSkyboxDir(moon.Target.position, moon.Height);

        float t = 0;
        while (t <= MoonMoveDuration)
        {
            t += Time.deltaTime;
            moonDir = Vector3.Lerp(currentDir, targetDir, t / MoonMoveDuration);
            yield return null;
        }
    }

    public void SetDayNightTransition(float t)
    {
        if(GameManager.Get().IsDay)
        {
            daySun.colorTemperature = Mathf.Lerp(nightTemp, dayTemp, t);
            daySun.intensity = Mathf.Lerp(nightIntensity, dayIntensity, t);
            daySun.color = Color.Lerp(nightColor, dayColor, t);
        }
        else
        {
            nightSun.colorTemperature = Mathf.Lerp(dayTemp, nightTemp, t);
            nightSun.intensity = Mathf.Lerp(dayIntensity, nightIntensity, t);
            nightSun.color = Color.Lerp(dayColor, nightColor, t);
        }
    }
}
