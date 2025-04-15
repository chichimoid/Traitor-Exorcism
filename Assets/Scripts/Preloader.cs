using UnityEngine;
public class Preloader : MonoBehaviour
{
    void Start()
    {
        SceneLoader.Instance.LoadSceneLocal(SceneLoader.Scene.MainMenu);
    }
}
