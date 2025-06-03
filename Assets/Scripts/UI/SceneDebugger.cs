using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool logScenesOnStart = true;
    
    private void Start()
    {
        if (logScenesOnStart)
        {
            LogBuildSettings();
        }
    }
    
    [ContextMenu("Log Build Settings")]
    public void LogBuildSettings()
    {
        Debug.Log("=== SCENE DEBUG INFO ===");
        Debug.Log("Total scenes in build: " + SceneManager.sceneCountInBuildSettings);
        Debug.Log("Current scene: " + SceneManager.GetActiveScene().name);
        
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"Build Index {i}: {sceneName} (Path: {scenePath})");
        }
        
        Debug.Log("=== END DEBUG INFO ===");
    }
    
    [ContextMenu("Test Load New Scene1")]
    public void TestLoadNewScene1()
    {
        string sceneName = "New Scene1";
        Debug.Log("Testing load of scene: " + sceneName);
        
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log("Scene can be loaded!");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene cannot be loaded! Check Build Settings.");
        }
    }
    
    [ContextMenu("Load Scene By Index 1")]
    public void LoadSceneByIndex1()
    {
        Debug.Log("Loading scene by index 1");
        if (SceneManager.sceneCountInBuildSettings > 1)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.LogError("No scene at index 1!");
        }
    }
} 