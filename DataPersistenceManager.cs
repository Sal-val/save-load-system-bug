using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool disableDataPersistence = false;
    [SerializeField] private bool initializeDataIfNull = false;
    [SerializeField] private bool overrideSelectedProfileId = false;
    [SerializeField] private string testSelectedProfileId = "test";

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    private Attack _attack;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    private string selectedProfileId = "";
    private PlayerHealth _playerHealth;

    public static DataPersistenceManager instance { get; private set; }
    private void Awake()
    {
        if(instance != null)
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying the newest one.");
            Destroy(gameObject);
            return;
        } else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);

        InitializeSelectedProfileId();
        _playerHealth = PlayerController.Instance.GetComponentInParent<PlayerHealth>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
            Debug.Log("OnSceneLoaded Called");
            this.dataPersistenceObjects = FindAllDataPersistenceObjects();
            LoadGame();
            AudioManager.Instance.GetComponent<MusicChangeTrigger>().CheckSceneMusic();
    }

    public void ChangeSelectedProfileId(string newProfileId)
    {
        this.selectedProfileId = newProfileId;
        LoadGame();
        Debug.Log("Game loaded at ChangeSelectedProfileData. ProfileId: " + newProfileId);
    }
    public void DeleteProfileData(string profileId)
    {
        dataHandler.Delete(profileId);

        InitializeSelectedProfileId();
        LoadGame();
        Debug.Log("Game loaded at DeleteProfileData");
    }

    private void InitializeSelectedProfileId()
    {
        this.selectedProfileId = dataHandler.GetMostRecentlyUpdatedProfileId();
        if(overrideSelectedProfileId)
        {
            this.selectedProfileId = testSelectedProfileId;
            Debug.Log("Overrode selected profile id with test id: " + testSelectedProfileId);
        }
    }

    public void NewGame()
    {
        this.gameData = new GameData();
        Debug.Log("New GameData assigned");
        PlayerController.Instance._activeScene = "Level_1";
        _playerHealth._startingHealth = 3;
        _playerHealth._currentHealth = 3;
    }

    public void LoadGame()
    {
        if(disableDataPersistence)
        {
            return;
        }

        this.gameData = dataHandler.Load(selectedProfileId);

        if(this.gameData == null && initializeDataIfNull)
        {
            NewGame();
        }

        if(this.gameData == null)
        {
            Debug.Log("No data was found. New Game needs to be started before data can be loaded.");
            return;
        }

        foreach (IDataPersistence dataPersistanceObj in dataPersistenceObjects)
        {
            dataPersistanceObj.LoadData(gameData);
            Debug.Log("Loaded DPObj: " + dataPersistanceObj);

        }

        Debug.Log("(LoadGame-DPM)Game Loaded");
        PlayerController.Instance.isGamePaused = false;
        PlayerController.Instance.GetComponentInChildren<Attack>().CreateBulletPool();
    }

    public void SaveGame()
    {
        if(disableDataPersistence)
        {
            return;
        }

        if (this.gameData == null)
        {
            Debug.Log("No data was found. A New Game needs to be started before data can be saved.");
            return;
        }

        foreach (IDataPersistence dataPersistanceObj in dataPersistenceObjects)
        {
            dataPersistanceObj.SaveData(gameData);
            Debug.Log("Saved DPObj: " + dataPersistanceObj);
        }
        gameData.lastUpdated = System.DateTime.Now.ToBinary();
        dataHandler.Save(gameData, selectedProfileId);
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public bool HasGameData()
    {
        return gameData != null;
    }

    public Dictionary<string, GameData> GetAllProfilesGameData()
    {
        return dataHandler.LoadAllProfiles();
    }

    public string GetSavedSceneName()
    {
        if(gameData == null)
        {
            Debug.Log("Tried to get scene name but data was null.");
            return null;
        }

        Debug.Log("GetSavedSceneName: " + gameData.currentSceneSG);
        return gameData.currentSceneSG;
        
    }
}
