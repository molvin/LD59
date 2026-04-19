using UnityEngine;

[ExecuteAlways]
public class SkyboxController : MonoBehaviour
{
    [SerializeField] private Material skyboxMaterial;

    void OnEnable()
    {
        if (skyboxMaterial != null)
            RenderSettings.skybox = skyboxMaterial;
    }
}
