﻿using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
    }
    public enum Scene
    {
        MainMenu,
        Lobby,
        Maze,
        Voting
    }

    public void LoadSceneLocal(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }
    public void LoadSceneGlobal(Scene scene)
    {
        NetworkManager.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }
}