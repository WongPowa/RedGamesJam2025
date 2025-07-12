using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighscoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject highscorePanel;
    [SerializeField] private Transform scoreListParent;
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private Text personalBestText;
    [SerializeField] private Text totalGamesText;
    [SerializeField] private Text totalTimeText;
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private Button submitScoreButton;
    [SerializeField] private Button showHighscoresButton;
    [SerializeField] private Button clearDataButton;
    
    [Header("Test Settings")]
    [SerializeField] private int testScore = 1000;
    
    void Start()
    {
        SetupUI();
        UpdateDisplays();
    }
    
    void SetupUI()
    {
        if (submitScoreButton != null)
            submitScoreButton.onClick.AddListener(SubmitTestScore);
            
        if (showHighscoresButton != null)
            showHighscoresButton.onClick.AddListener(ToggleHighscorePanel);
            
        if (clearDataButton != null)
            clearDataButton.onClick.AddListener(ClearAllData);
    }
    
    void OnEnable()
    {
        if (HighscoreManager.Instance != null)
        {
            HighscoreManager.Instance.OnNewPersonalBest += OnNewPersonalBest;
            HighscoreManager.Instance.OnNewHighscore += OnNewHighscore;
        }
    }
    
    void OnDisable()
    {
        if (HighscoreManager.Instance != null)
        {
            HighscoreManager.Instance.OnNewPersonalBest -= OnNewPersonalBest;
            HighscoreManager.Instance.OnNewHighscore -= OnNewHighscore;
        }
    }
    
    void OnNewPersonalBest(int newBest)
    {
        Debug.Log($"NEW PERSONAL BEST: {newBest}!");
        UpdateDisplays();
    }
    
    void OnNewHighscore(HighscoreEntry entry)
    {
        Debug.Log($"New highscore entry: {entry.playerName} - {entry.score}");
        UpdateHighscoreList();
    }
    
    public void SubmitTestScore()
    {
        if (HighscoreManager.Instance == null) return;
        
        string playerName = playerNameInput != null ? playerNameInput.text : "Player";
        HighscoreManager.Instance.SubmitScore(playerName, testScore);
        
        testScore += Random.Range(50, 200);
    }
    
    public void SubmitScore(int score)
    {
        if (HighscoreManager.Instance == null) return;
        
        string playerName = playerNameInput != null ? playerNameInput.text : "Player";
        HighscoreManager.Instance.SubmitScore(playerName, score);
    }
    
    public void ToggleHighscorePanel()
    {
        if (highscorePanel != null)
        {
            bool isActive = !highscorePanel.activeSelf;
            highscorePanel.SetActive(isActive);
            
            if (isActive)
            {
                UpdateHighscoreList();
            }
        }
    }
    
    void UpdateDisplays()
    {
        if (HighscoreManager.Instance == null) return;
        
        if (personalBestText != null)
            personalBestText.text = $"Personal Best: {HighscoreManager.Instance.GetPersonalBest()}";
            
        if (totalGamesText != null)
            totalGamesText.text = $"Games Played: {HighscoreManager.Instance.GetTotalGamesPlayed()}";
            
        if (totalTimeText != null)
        {
            float totalTime = HighscoreManager.Instance.GetTotalPlayTime();
            int minutes = Mathf.FloorToInt(totalTime / 60);
            int seconds = Mathf.FloorToInt(totalTime % 60);
            totalTimeText.text = $"Total Play Time: {minutes}m {seconds}s";
        }
    }
    
    void UpdateHighscoreList()
    {
        if (HighscoreManager.Instance == null || scoreListParent == null) return;
        
        foreach (Transform child in scoreListParent)
        {
            Destroy(child.gameObject);
        }
        
        List<HighscoreEntry> topScores = HighscoreManager.Instance.GetTopScores(10);
        
        for (int i = 0; i < topScores.Count; i++)
        {
            GameObject entryObj = CreateScoreEntry(i + 1, topScores[i]);
            if (entryObj != null)
                entryObj.transform.SetParent(scoreListParent, false);
        }
    }
    
    GameObject CreateScoreEntry(int rank, HighscoreEntry entry)
    {
        GameObject entryObj;
        
        if (scoreEntryPrefab != null)
        {
            entryObj = Instantiate(scoreEntryPrefab);
        }
        else
        {
            entryObj = new GameObject($"Score Entry {rank}");
            entryObj.AddComponent<Text>();
        }
        
        Text entryText = entryObj.GetComponent<Text>();
        if (entryText != null)
        {
            entryText.text = $"{rank}. {entry.playerName} - {entry.score}";
            entryText.fontSize = 14;
            entryText.color = Color.white;
        }
        
        return entryObj;
    }
    
    public void ClearAllData()
    {
        if (HighscoreManager.Instance != null)
        {
            HighscoreManager.Instance.ClearAllData();
            UpdateDisplays();
            UpdateHighscoreList();
        }
    }
} 