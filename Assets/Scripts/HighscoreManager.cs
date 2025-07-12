using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class HighscoreEntry
{
    public string playerName;
    public int score;
    public float timeAchieved;
    public DateTime dateAchieved;
    
    public HighscoreEntry(string name, int playerScore)
    {
        playerName = name;
        score = playerScore;
        timeAchieved = Time.time;
        dateAchieved = DateTime.Now;
    }
}

[System.Serializable]
public class HighscoreData
{
    public List<HighscoreEntry> entries = new List<HighscoreEntry>();
    public int personalBest;
    public int totalGamesPlayed;
    public float totalPlayTime;
}

public class HighscoreManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxLocalEntries = 100;
    [SerializeField] private bool useJsonStorage = true;
    [SerializeField] private bool usePlayerPrefs = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private HighscoreData localData;
    private string saveFilePath;
    private const string PREFS_PERSONAL_BEST = "PersonalBest";
    private const string PREFS_TOTAL_GAMES = "TotalGames";
    private const string PREFS_TOTAL_TIME = "TotalPlayTime";
    
    public static HighscoreManager Instance { get; private set; }
    
    public event System.Action<int> OnNewPersonalBest;
    public event System.Action<HighscoreEntry> OnNewHighscore;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeManager()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "highscores.json");
        localData = new HighscoreData();
        LoadHighscores();
    }
    
    public void SubmitScore(string playerName, int score)
    {
        if (string.IsNullOrEmpty(playerName))
            playerName = "Anonymous";
            
        HighscoreEntry newEntry = new HighscoreEntry(playerName, score);
        
        bool isPersonalBest = score > localData.personalBest;
        if (isPersonalBest)
        {
            localData.personalBest = score;
            OnNewPersonalBest?.Invoke(score);
        }
        
        localData.entries.Add(newEntry);
        localData.entries.Sort((a, b) => b.score.CompareTo(a.score));
        
        if (localData.entries.Count > maxLocalEntries)
        {
            localData.entries.RemoveAt(localData.entries.Count - 1);
        }
        
        localData.totalGamesPlayed++;
        
        OnNewHighscore?.Invoke(newEntry);
        SaveHighscores();
        
        if (showDebugLogs)
        {
            Debug.Log($"New score submitted: {playerName} - {score}" + (isPersonalBest ? " (NEW PERSONAL BEST!)" : ""));
        }
    }
    
    public List<HighscoreEntry> GetTopScores(int count = 10)
    {
        count = Mathf.Min(count, localData.entries.Count);
        return localData.entries.GetRange(0, count);
    }
    
    public int GetPersonalBest()
    {
        return localData.personalBest;
    }
    
    public int GetPlayerRank(int score)
    {
        for (int i = 0; i < localData.entries.Count; i++)
        {
            if (score >= localData.entries[i].score)
                return i + 1;
        }
        return localData.entries.Count + 1;
    }
    
    public HighscoreEntry GetPlayerBestEntry()
    {
        foreach (var entry in localData.entries)
        {
            if (entry.score == localData.personalBest)
                return entry;
        }
        return null;
    }
    
    public void AddPlayTime(float sessionTime)
    {
        localData.totalPlayTime += sessionTime;
        SaveHighscores();
    }
    
    public float GetTotalPlayTime()
    {
        return localData.totalPlayTime;
    }
    
    public int GetTotalGamesPlayed()
    {
        return localData.totalGamesPlayed;
    }
    
    void SaveHighscores()
    {
        if (useJsonStorage)
        {
            SaveToJson();
        }
        
        if (usePlayerPrefs)
        {
            SaveToPlayerPrefs();
        }
    }
    
    void LoadHighscores()
    {
        bool dataLoaded = false;
        
        if (useJsonStorage && File.Exists(saveFilePath))
        {
            dataLoaded = LoadFromJson();
        }
        
        if (!dataLoaded && usePlayerPrefs)
        {
            LoadFromPlayerPrefs();
        }
    }
    
    void SaveToJson()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(localData, true);
            File.WriteAllText(saveFilePath, jsonData);
            
            if (showDebugLogs)
                Debug.Log("Highscores saved to JSON");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save highscores to JSON: {e.Message}");
        }
    }
    
    bool LoadFromJson()
    {
        try
        {
            string jsonData = File.ReadAllText(saveFilePath);
            localData = JsonUtility.FromJson<HighscoreData>(jsonData);
            
            if (localData.entries == null)
                localData.entries = new List<HighscoreEntry>();
                
            if (showDebugLogs)
                Debug.Log($"Highscores loaded from JSON: {localData.entries.Count} entries");
                
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load highscores from JSON: {e.Message}");
            localData = new HighscoreData();
            return false;
        }
    }
    
    void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt(PREFS_PERSONAL_BEST, localData.personalBest);
        PlayerPrefs.SetInt(PREFS_TOTAL_GAMES, localData.totalGamesPlayed);
        PlayerPrefs.SetFloat(PREFS_TOTAL_TIME, localData.totalPlayTime);
        PlayerPrefs.Save();
    }
    
    void LoadFromPlayerPrefs()
    {
        localData.personalBest = PlayerPrefs.GetInt(PREFS_PERSONAL_BEST, 0);
        localData.totalGamesPlayed = PlayerPrefs.GetInt(PREFS_TOTAL_GAMES, 0);
        localData.totalPlayTime = PlayerPrefs.GetFloat(PREFS_TOTAL_TIME, 0f);
        
        if (showDebugLogs)
            Debug.Log($"Basic data loaded from PlayerPrefs: PB={localData.personalBest}");
    }
    
    public void ClearAllData()
    {
        localData = new HighscoreData();
        
        if (File.Exists(saveFilePath))
            File.Delete(saveFilePath);
            
        PlayerPrefs.DeleteKey(PREFS_PERSONAL_BEST);
        PlayerPrefs.DeleteKey(PREFS_TOTAL_GAMES);
        PlayerPrefs.DeleteKey(PREFS_TOTAL_TIME);
        PlayerPrefs.Save();
        
        Debug.Log("All highscore data cleared");
    }
    
    public HighscoreData ExportDataForUpload()
    {
        return localData;
    }
    
    public void ImportDataFromServer(HighscoreData serverData)
    {
        if (serverData != null)
        {
            localData = serverData;
            SaveHighscores();
        }
    }
} 