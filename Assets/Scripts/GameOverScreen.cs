using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    
    [Header("Score Display")]
    [SerializeField] private Text currentScoreText;
    [SerializeField] private Text personalBestText;
    [SerializeField] private Text heightText;
    
    [Header("Leaderboard")]
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Text leaderboardTitle;
    [SerializeField] private int maxLeaderboardEntries = 20;
    
    [Header("Leaderboard Colors")]
    [SerializeField] private Color currentPlayerColor = Color.yellow;
    [SerializeField] private Color normalEntryColor = Color.white;
    [SerializeField] private Color headerColor = Color.cyan;
    
    [Header("Buttons")]
    [SerializeField] private Button tryAgainButton;
    
    [Header("Animation Settings")]
    [SerializeField] private bool useAnimations = true;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float delayBeforeShow = 0.3f;
    
    private List<GameObject> leaderboardEntries = new List<GameObject>();
    private int currentPlayerScore = 0;
    private string currentPlayerName = "";
    
    public static GameOverScreen Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SetupUI();
        
        // Initially hide the screen
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    void SetupUI()
    {
        // Setup canvas group for animations
        if (gameOverCanvasGroup == null && gameOverPanel != null)
        {
            gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (gameOverCanvasGroup == null)
            {
                gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }
        }
        
        // Setup buttons
        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(TryAgain);
            
        // Setup leaderboard title
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
        RectTransform entryRect = entryPrefab.AddComponent<RectTransform>();
        entryRect.sizeDelta = new Vector2(400, 25);
        
        // Add horizontal layout
        HorizontalLayoutGroup layout = entryPrefab.AddComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 10f;
        layout.padding = new RectOffset(5, 5, 2, 2);
        
        // Create rank text
        GameObject rankObj = new GameObject("Rank");
        rankObj.transform.SetParent(entryPrefab.transform);
        Text rankTxt = rankObj.AddComponent<Text>();
        rankTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        rankTxt.fontSize = 14;
        rankTxt.color = Color.white;
        rankTxt.text = "1.";
        rankTxt.alignment = TextAnchor.MiddleCenter;
        RectTransform rankRect = rankObj.GetComponent<RectTransform>();
        rankRect.sizeDelta = new Vector2(30, 20);
        
        // Create name text
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(entryPrefab.transform);
        Text nameTxt = nameObj.AddComponent<Text>();
        nameTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameTxt.fontSize = 14;
        nameTxt.color = Color.white;
        nameTxt.text = "Player";
        nameTxt.alignment = TextAnchor.MiddleLeft;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(200, 20);
        
        // Create score text
        GameObject scoreObj = new GameObject("Score");
        scoreObj.transform.SetParent(entryPrefab.transform);
        Text scoreTxt = scoreObj.AddComponent<Text>();
        scoreTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        scoreTxt.fontSize = 14;
        scoreTxt.color = Color.white;
        scoreTxt.text = "1000";
        scoreTxt.alignment = TextAnchor.MiddleRight;
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.sizeDelta = new Vector2(100, 20);
        
        leaderboardEntryPrefab = entryPrefab;
        
        Debug.Log("Created default leaderboard entry prefab");
    }
    
    public void ShowGameOverScreen(int score, string playerName)
    {
        if (gameOverPanel == null) return;
        
        currentPlayerScore = score;
        currentPlayerName = playerName;
        
        // Show the panel
        gameOverPanel.SetActive(true);
        
        // Update displays
        UpdateScoreDisplay();
        UpdateLeaderboard();
        
        // Show with animation or immediately
        if (useAnimations)
        {
            StartCoroutine(ShowWithDelay());
        }
        else
        {
            if (gameOverCanvasGroup != null)
                gameOverCanvasGroup.alpha = 1f;
        }
    }
    
    public void HideGameOverScreen()
    {
        if (useAnimations && gameOverCanvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }
    }
    
    void UpdateScoreDisplay()
    {
        // Current score
        if (currentScoreText != null)
            currentScoreText.text = $"Your Score: {currentPlayerScore}";
        
        // Height information
        if (heightText != null && GameSession.Instance != null)
        {
            float maxHeight = GameSession.Instance.MaxHeightReached;
            float startHeight = GameSession.Instance.StartingHeight;
            float heightGained = maxHeight - startHeight;
            heightText.text = $"Height Reached: {heightGained:F1}m";
        }
        
        // Personal best
        if (personalBestText != null && HighscoreManager.Instance != null)
        {
            int personalBest = HighscoreManager.Instance.GetPersonalBest();
            if (currentPlayerScore > personalBest)
            {
                personalBestText.text = "NEW PERSONAL BEST!";
                personalBestText.color = Color.yellow;
            }
            else
            {
                personalBestText.text = $"Personal Best: {personalBest}";
                personalBestText.color = Color.white;
            }
        }
    }
    
    void UpdateLeaderboard()
    {
        ClearLeaderboard();
        
        if (HighscoreManager.Instance == null || leaderboardContainer == null)
            return;
        
        // Get top scores
        List<HighscoreEntry> topScores = HighscoreManager.Instance.GetTopScores(maxLeaderboardEntries);
        
        // Create header
        CreateLeaderboardHeader();
        
        // Create entries
        for (int i = 0; i < topScores.Count; i++)
        {
            CreateLeaderboardEntry(i + 1, topScores[i]);
        }
        
        Debug.Log($"Leaderboard updated with {topScores.Count} entries");
    }
    
    void CreateLeaderboardHeader()
    {
        if (leaderboardContainer == null || leaderboardEntryPrefab == null)
            return;
            
        GameObject headerObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        leaderboardEntries.Add(headerObj);
        
        // Find text components
        Text[] textComponents = headerObj.GetComponentsInChildren<Text>();
        
        if (textComponents.Length >= 3)
        {
            textComponents[0].text = "#";
            textComponents[1].text = "Name";
            textComponents[2].text = "Score";
            
            // Style as header
            foreach (Text txt in textComponents)
            {
                txt.color = headerColor;
                txt.fontStyle = FontStyle.Bold;
            }
        }
    }
    
    void CreateLeaderboardEntry(int rank, HighscoreEntry entry)
    {
        if (leaderboardContainer == null || leaderboardEntryPrefab == null)
            return;
            
        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        leaderboardEntries.Add(entryObj);
        
        // Find text components
        Text[] textComponents = entryObj.GetComponentsInChildren<Text>();
        
        if (textComponents.Length >= 3)
        {
            // Rank
            textComponents[0].text = $"{rank}";
            
            // Name
            textComponents[1].text = entry.playerName;
            
            // Score
            textComponents[2].text = entry.score.ToString();
            
            // Highlight if this is the current player's score
            bool isCurrentPlayer = entry.playerName == currentPlayerName && entry.score == currentPlayerScore;
            Color entryColor = isCurrentPlayer ? currentPlayerColor : normalEntryColor;
            
            foreach (Text txt in textComponents)
            {
                txt.color = entryColor;
                if (isCurrentPlayer)
                {
                    txt.fontStyle = FontStyle.Bold;
                }
            }
        }
    }
    
    void ClearLeaderboard()
    {
        foreach (GameObject entry in leaderboardEntries)
        {
            if (entry != null)
            {
                DestroyImmediate(entry);
            }
        }
        leaderboardEntries.Clear();
    }
    
    void TryAgain()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.RetryGame();
        }
        
        HideGameOverScreen();
    }
    
    System.Collections.IEnumerator ShowWithDelay()
    {
        if (gameOverCanvasGroup != null)
            gameOverCanvasGroup.alpha = 0f;
        
        // Wait for delay
        yield return new WaitForSeconds(delayBeforeShow);
        
        // Fade in
        yield return StartCoroutine(FadeIn());
    }
    
    System.Collections.IEnumerator FadeIn()
    {
        if (gameOverCanvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            gameOverCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            yield return null;
        }
        
        gameOverCanvasGroup.alpha = 1f;
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        if (gameOverCanvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            gameOverCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeInDuration));
            yield return null;
        }
        
        gameOverCanvasGroup.alpha = 0f;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
} 