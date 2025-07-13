using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    
    [Header("Score Display")]
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text personalBestText;
    [SerializeField] private Text rankText;
    [SerializeField] private Text heightText;
    
    [Header("Leaderboard")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private LeaderboardUI leaderboardUI;
    [SerializeField] private Button showLeaderboardButton;
    [SerializeField] private Button hideLeaderboardButton;
    
    [Header("Player Input")]
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private Button submitScoreButton;
    
    [Header("Navigation Buttons")]
    [SerializeField] private Button retryButton;
    
    [Header("Animation Settings")]
    [SerializeField] private bool useAnimations = true;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float delayBeforeShow = 1f;
    
    private bool scoreSubmitted = false;
    private bool isLeaderboardVisible = false;
    
    void Start()
    {
        SetupUI();
        
        // Subscribe to game events
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnGameEnd += ShowGameOverScreen;
            GameSession.Instance.OnGameStart += HideGameOverScreen;
        }
        
        // Initially hide the game over screen
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
        if (submitScoreButton != null)
            submitScoreButton.onClick.AddListener(SubmitScore);
            
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);
            
        if (showLeaderboardButton != null)
            showLeaderboardButton.onClick.AddListener(ShowLeaderboard);
            
        if (hideLeaderboardButton != null)
            hideLeaderboardButton.onClick.AddListener(HideLeaderboard);
            
        // Setup leaderboard
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
            
        if (leaderboardUI == null && leaderboardPanel != null)
        {
            leaderboardUI = leaderboardPanel.GetComponent<LeaderboardUI>();
        }
    }
    
    public void ShowGameOverScreen()
    {
        if (gameOverPanel == null) return;
        
        scoreSubmitted = false;
        
        // Show the panel
        gameOverPanel.SetActive(true);
        
        // Update score display
        UpdateScoreDisplay();
        
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
        
        HideLeaderboard();
    }
    
    void UpdateScoreDisplay()
    {
        if (GameSession.Instance == null) return;
        
        int currentScore = GameSession.Instance.CurrentScore;
        
        // Final score
        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {currentScore}";
        
        // Height information
        if (heightText != null && GameSession.Instance.PlayerTransform != null)
        {
            float maxHeight = GameSession.Instance.MaxHeightReached;
            float startHeight = GameSession.Instance.StartingHeight;
            float heightGained = maxHeight - startHeight;
            heightText.text = $"Height Reached: {heightGained:F1}m";
        }
        
        // Personal best and rank
        if (HighscoreManager.Instance != null)
        {
            int personalBest = HighscoreManager.Instance.GetPersonalBest();
            int rank = HighscoreManager.Instance.GetPlayerRank(currentScore);
            
            if (personalBestText != null)
            {
                if (currentScore > personalBest)
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
            
            if (rankText != null)
                rankText.text = $"Global Rank: #{rank}";
        }
        
        // Reset submit button
        if (submitScoreButton != null)
        {
            submitScoreButton.interactable = true;
            Text buttonText = submitScoreButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = "Submit Score";
        }
    }
    
    public void SubmitScore()
    {
        if (scoreSubmitted || GameSession.Instance == null) return;
        
        string playerName = "Player";
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
            playerName = playerNameInput.text;
        
        // Submit through GameSession
        GameSession.Instance.SubmitCurrentScore();
        scoreSubmitted = true;
        
        // Update button
        if (submitScoreButton != null)
        {
            submitScoreButton.interactable = false;
            Text buttonText = submitScoreButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = "Score Submitted";
        }
        
        // Refresh leaderboard if visible
        if (isLeaderboardVisible && leaderboardUI != null)
        {
            leaderboardUI.RefreshLeaderboard();
        }
    }
    
    public void RetryGame()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.RetryGame();
        }
    }
    
    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null && leaderboardUI != null)
        {
            leaderboardPanel.SetActive(true);
            leaderboardUI.ShowLeaderboard();
            isLeaderboardVisible = true;
            
            if (showLeaderboardButton != null)
                showLeaderboardButton.gameObject.SetActive(false);
            if (hideLeaderboardButton != null)
                hideLeaderboardButton.gameObject.SetActive(true);
        }
    }
    
    public void HideLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
            isLeaderboardVisible = false;
            
            if (showLeaderboardButton != null)
                showLeaderboardButton.gameObject.SetActive(true);
            if (hideLeaderboardButton != null)
                hideLeaderboardButton.gameObject.SetActive(false);
        }
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
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnGameEnd -= ShowGameOverScreen;
            GameSession.Instance.OnGameStart -= HideGameOverScreen;
        }
    }
} 