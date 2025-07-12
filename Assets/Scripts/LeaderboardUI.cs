using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Leaderboard Settings")]
    [SerializeField] private int maxEntriesToShow = 20;
    [SerializeField] private bool highlightPlayerScore = true;
    [SerializeField] private Color playerScoreColor = Color.yellow;
    [SerializeField] private Color normalScoreColor = Color.white;
    
    [Header("UI References")]
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Text leaderboardTitle;
    [SerializeField] private Button refreshButton;
    
    [Header("Entry Prefab Components (for reference)")]
    [SerializeField] private Text rankText;
    [SerializeField] private Text nameText;
    [SerializeField] private Text scoreText;
    
    private List<GameObject> currentEntries = new List<GameObject>();
    private int currentPlayerScore = 0;
    
    void Start()
    {
        SetupUI();
        
        // Subscribe to game events
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnGameEnd += RefreshLeaderboard;
        }
    }
    
    void SetupUI()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshLeaderboard);
            
        if (leaderboardTitle != null)
            leaderboardTitle.text = "Leaderboard";
            
        // Create entry prefab if it doesn't exist
        if (leaderboardEntryPrefab == null)
        {
            CreateDefaultEntryPrefab();
        }
    }
    
    void CreateDefaultEntryPrefab()
    {
        // Create a simple entry prefab programmatically
        GameObject entryPrefab = new GameObject("LeaderboardEntry");
        entryPrefab.AddComponent<RectTransform>();
        
        // Add horizontal layout
        HorizontalLayoutGroup layout = entryPrefab.AddComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 10f;
        
        // Create rank text
        GameObject rankObj = new GameObject("Rank");
        rankObj.transform.SetParent(entryPrefab.transform);
        Text rankTxt = rankObj.AddComponent<Text>();
        rankTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        rankTxt.fontSize = 16;
        rankTxt.color = Color.white;
        rankTxt.text = "1.";
        RectTransform rankRect = rankObj.GetComponent<RectTransform>();
        rankRect.sizeDelta = new Vector2(30, 20);
        
        // Create name text
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(entryPrefab.transform);
        Text nameTxt = nameObj.AddComponent<Text>();
        nameTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameTxt.fontSize = 16;
        nameTxt.color = Color.white;
        nameTxt.text = "Player";
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(120, 20);
        
        // Create score text
        GameObject scoreObj = new GameObject("Score");
        scoreObj.transform.SetParent(entryPrefab.transform);
        Text scoreTxt = scoreObj.AddComponent<Text>();
        scoreTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        scoreTxt.fontSize = 16;
        scoreTxt.color = Color.white;
        scoreTxt.text = "1000";
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.sizeDelta = new Vector2(80, 20);
        
        leaderboardEntryPrefab = entryPrefab;
        
        Debug.Log("Created default leaderboard entry prefab");
    }
    
    public void RefreshLeaderboard()
    {
        ClearCurrentEntries();
        
        if (HighscoreManager.Instance == null)
        {
            Debug.LogWarning("HighscoreManager not found - cannot display leaderboard");
            return;
        }
        
        // Get current player score
        if (GameSession.Instance != null)
        {
            currentPlayerScore = GameSession.Instance.CurrentScore;
        }
        
        // Get top scores
        List<HighscoreEntry> topScores = HighscoreManager.Instance.GetTopScores(maxEntriesToShow);
        
        // Create UI entries
        for (int i = 0; i < topScores.Count; i++)
        {
            CreateLeaderboardEntry(i + 1, topScores[i]);
        }
        
        Debug.Log($"Leaderboard refreshed with {topScores.Count} entries");
    }
    
    void CreateLeaderboardEntry(int rank, HighscoreEntry entry)
    {
        if (leaderboardContainer == null || leaderboardEntryPrefab == null)
            return;
            
        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        currentEntries.Add(entryObj);
        
        // Find text components
        Text[] textComponents = entryObj.GetComponentsInChildren<Text>();
        
        if (textComponents.Length >= 3)
        {
            // Rank
            textComponents[0].text = $"{rank}.";
            
            // Name
            textComponents[1].text = entry.playerName;
            
            // Score
            textComponents[2].text = entry.score.ToString();
            
            // Highlight if this is the current player's score
            if (highlightPlayerScore && entry.score == currentPlayerScore)
            {
                foreach (Text txt in textComponents)
                {
                    txt.color = playerScoreColor;
                }
            }
            else
            {
                foreach (Text txt in textComponents)
                {
                    txt.color = normalScoreColor;
                }
            }
        }
        
        // Set proper anchoring and positioning
        RectTransform rectTransform = entryObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, -rank * 25f);
            rectTransform.sizeDelta = new Vector2(0, 20);
        }
    }
    
    void ClearCurrentEntries()
    {
        foreach (GameObject entry in currentEntries)
        {
            if (entry != null)
            {
                DestroyImmediate(entry);
            }
        }
        currentEntries.Clear();
    }
    
    public void ShowLeaderboard()
    {
        gameObject.SetActive(true);
        RefreshLeaderboard();
    }
    
    public void HideLeaderboard()
    {
        gameObject.SetActive(false);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnGameEnd -= RefreshLeaderboard;
        }
    }
} 