using UnityEngine;

public class SceneConfig : MonoBehaviour
{
    public bool SetupPlayScene = false;

    public static bool InitializePlayScene()
    {
        bool result = true;

        var Config = FindFirstObjectByType<SceneConfig>();
        if (Config != null)
        {
            result = Config.SetupPlayScene;
        }

        return result;
    }
}
