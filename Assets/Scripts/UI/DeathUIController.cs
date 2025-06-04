using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathUIController : MonoBehaviour
{
    [Header("Death UI Settings")]
    [Tooltip("UI изображение AllPigs для показа при смерти")]
    public GameObject allPigsImage;
    
    [Tooltip("Компонент Health свиньи/игрока")]
    public Health playerHealth;
    
    [Header("Audio Settings")]
    [Tooltip("AudioSource для проигрывания музыки смерти")]
    public AudioSource deathAudioSource;
    
    [Tooltip("Аудиоклип для проигрывания при смерти")]
    public AudioClip deathMusicClip;
    
    [Tooltip("Громкость музыки смерти")]
    [Range(0f, 1f)]
    public float deathMusicVolume = 0.8f;
    
    [Header("Auto Find Settings")]
    [Tooltip("Автоматически найти компоненты при старте")]
    public bool autoFindComponents = true;
    
    [Header("Debug")]
    [Tooltip("Показывать отладочную информацию")]
    public bool showDebugInfo = false;
    
    // Private variables
    private bool hasPlayedDeathMusic = false;
    private Image allPigsImageComponent;
    
    void Start()
    {
        if (autoFindComponents)
        {
            AutoFindComponents();
        }
        
        SetupDeathUI();
        SubscribeToDeathEvent();
    }
    
    private void AutoFindComponents()
    {
        Debug.Log("DeathUIController: Автопоиск компонентов...");
        
        // Find AllPigs image
        if (allPigsImage == null)
        {
            allPigsImage = FindObjectByName("AllPigs");
            if (allPigsImage != null)
            {
                Debug.Log($"DeathUIController: Найден AllPigs: {allPigsImage.name}");
            }
            else
            {
                Debug.LogWarning("DeathUIController: AllPigs изображение не найдено!");
            }
        }
        
        // Find player health
        if (playerHealth == null)
        {
            // Try to find by Player tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
            
            // If not found, search all Health components
            if (playerHealth == null)
            {
                Health[] allHealthComponents = FindObjectsOfType<Health>();
                foreach (Health health in allHealthComponents)
                {
                    string name = health.gameObject.name.ToLower();
                    if (name.Contains("pig") || name.Contains("player") || name.Contains("свин"))
                    {
                        playerHealth = health;
                        Debug.Log($"DeathUIController: Найден Health компонент: {health.gameObject.name}");
                        break;
                    }
                }
            }
        }
        
        // Find or create AudioSource
        if (deathAudioSource == null)
        {
            deathAudioSource = GetComponent<AudioSource>();
            if (deathAudioSource == null)
            {
                deathAudioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("DeathUIController: Создан новый AudioSource");
            }
        }
    }
    
    private GameObject FindObjectByName(string objectName)
    {
        // Search in all GameObjects
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName)
            {
                return obj;
            }
        }
        return null;
    }
    
    private void SetupDeathUI()
    {
        // Setup AllPigs image
        if (allPigsImage != null)
        {
            allPigsImageComponent = allPigsImage.GetComponent<Image>();
            
            // Initially hide the death UI
            allPigsImage.SetActive(false);
            
            if (showDebugInfo)
            {
                Debug.Log("DeathUIController: AllPigs изображение настроено и скрыто");
            }
        }
        
        // Setup audio source
        if (deathAudioSource != null)
        {
            deathAudioSource.volume = deathMusicVolume;
            deathAudioSource.playOnAwake = false;
            deathAudioSource.loop = false;
            
            if (deathMusicClip != null)
            {
                deathAudioSource.clip = deathMusicClip;
            }
        }
    }
    
    private void SubscribeToDeathEvent()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath.AddListener(OnPlayerDeath);
            
            if (showDebugInfo)
            {
                Debug.Log($"DeathUIController: Подписан на событие смерти {playerHealth.gameObject.name}");
            }
        }
        else
        {
            Debug.LogError("DeathUIController: Health компонент не найден! Назначьте его вручную или включите autoFindComponents");
        }
    }
    
    private void OnPlayerDeath()
    {
        if (showDebugInfo)
        {
            Debug.Log("DeathUIController: Получено событие смерти игрока!");
        }
        
        ShowDeathUI();
        PlayDeathMusic();
    }
    
    private void ShowDeathUI()
    {
        if (allPigsImage != null)
        {
            allPigsImage.SetActive(true);
            
            if (showDebugInfo)
            {
                Debug.Log("DeathUIController: Показано AllPigs изображение");
            }
        }
        else
        {
            Debug.LogError("DeathUIController: AllPigs изображение не назначено!");
        }
    }
    
    private void PlayDeathMusic()
    {
        if (deathAudioSource != null && deathMusicClip != null && !hasPlayedDeathMusic)
        {
            deathAudioSource.clip = deathMusicClip;
            deathAudioSource.volume = deathMusicVolume;
            deathAudioSource.Play();
            
            hasPlayedDeathMusic = true;
            
            if (showDebugInfo)
            {
                Debug.Log("DeathUIController: Запущена музыка смерти");
            }
        }
        else if (hasPlayedDeathMusic)
        {
            if (showDebugInfo)
            {
                Debug.Log("DeathUIController: Музыка смерти уже была проиграна");
            }
        }
        else
        {
            Debug.LogWarning("DeathUIController: AudioSource или аудиоклип не назначены!");
        }
    }
    
    // Public methods for external control
    public void HideDeathUI()
    {
        if (allPigsImage != null)
        {
            allPigsImage.SetActive(false);
            Debug.Log("DeathUIController: AllPigs изображение скрыто");
        }
    }
    
    public void ResetDeathMusic()
    {
        hasPlayedDeathMusic = false;
        Debug.Log("DeathUIController: Сброшен флаг музыки смерти");
    }
    
    public void StopDeathMusic()
    {
        if (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            deathAudioSource.Stop();
            Debug.Log("DeathUIController: Остановлена музыка смерти");
        }
    }
    
    // Context menu methods for testing
    [ContextMenu("Test Death UI")]
    public void TestDeathUI()
    {
        Debug.Log("DeathUIController: Тестирование UI смерти...");
        OnPlayerDeath();
    }
    
    [ContextMenu("Reset Death State")]
    public void ResetDeathState()
    {
        HideDeathUI();
        ResetDeathMusic();
        StopDeathMusic();
        Debug.Log("DeathUIController: Состояние смерти сброшено");
    }
    
    [ContextMenu("Find Components")]
    public void ManualFindComponents()
    {
        AutoFindComponents();
        SetupDeathUI();
        SubscribeToDeathEvent();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (playerHealth != null)
        {
            playerHealth.OnDeath.RemoveListener(OnPlayerDeath);
        }
    }
} 