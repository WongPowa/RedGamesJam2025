using UnityEngine;
using UnityEngine.UI;

public class GameSession : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private float sessionStartTime;
    [SerializeField] private bool gameActive = false;
    
    [Header("Scoring")]
    [SerializeField] private float heightMultiplier = 10f;
    [SerializeField] private bool showHeightDebug = false;
    [SerializeField] private float minimumScoreHeight = 0f; // Minimum height needed to start scoring
    
    [Header("Respawn Settings")]
    [SerializeField] private bool resetScoreOnRespawn = false;
    [SerializeField] private int scorePenaltyOnRespawn = 0;
    [SerializeField] private bool resetHeightProgressOnRespawn = false;
    
    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private Button submitScoreButton;
    [SerializeField] private Button retryButton; // Single retry button
    
    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;
    
    private float startingHeight = 0f;
    private float maxHeightReached = 0f;
    private bool scoreSubmitted = false;
    
    public static GameSession Instance { get; private set; }
    
    public int CurrentScore => currentScore;
    public bool IsGameActive => gameActive;
    public float MaxHeightReached => maxHeightReached;
    public float StartingHeight => startingHeight;
    public Transform PlayerTransform => playerTransform;
    
    public event System.Action<int> OnScoreChanged;
    public event System.Action OnGameStart;
    public event System.Action OnGameEnd;
    public event System.Action OnPlayerRespawn;
    
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
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
    }
    
    void Update()
    {
        if (gameActive)
        {
            UpdateScore();
            CheckGameEnd();
        }
    }
    
    void SetupUI()
    {
        if (submitScoreButton != null)
            submitScoreButton.onClick.AddListener(SubmitCurrentScore);
            
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);
            
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    public void StartGame()
    {
        gameActive = true;
        currentScore = 0;
        sessionStartTime = Time.time;
        startingHeight = playerTransform != null ? playerTransform.position.y : 0f;
        maxHeightReached = startingHeight;
        scoreSubmitted = false;
        CharacterMovement.Instance.LaunchCharacter();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        OnGameStart?.Invoke();
        UpdateScoreDisplay();
        
        Debug.Log("Game started!");
    }
    
    public void RetryGame()
    {
        // Reset player position and start new game
        RespawnPlayer();
        StartGame();
        Debug.Log("Game retried!");
    }
    
    public void RespawnPlayer()
    {
        if (playerTransform != null)
        {
            // Use CharacterMovement for respawn
            CharacterMovement characterMovement = playerTransform.GetComponent<CharacterMovement>();
            if (characterMovement != null)
            {
                characterMovement.RespawnPlayer();
            }
            else
            {
                Debug.LogWarning("No CharacterMovement component found on player!");
                return;
            }
            
            // Handle score changes on respawn
            if (resetScoreOnRespawn)
            {
                // Reset score and height tracking completely
                currentScore = 0;
                startingHeight = playerTransform.position.y; // Update starting height to current spawn point
                maxHeightReached = startingHeight;
            }
            else
            {
                // Handle height progress reset separately
                if (resetHeightProgressOnRespawn)
                {
                    // Reset height progress but keep current score
                    startingHeight = playerTransform.position.y;
                    maxHeightReached = startingHeight;
                }
                
                // Apply score penalty if configured
                if (scorePenaltyOnRespawn > 0)
                {
                    currentScore = Mathf.Max(0, currentScore - scorePenaltyOnRespawn);
                }
                
                // If neither reset nor penalty, player keeps everything
            }
            
            OnScoreChanged?.Invoke(currentScore);
            UpdateScoreDisplay();
            OnPlayerRespawn?.Invoke();
            
            Debug.Log($"Player respawned through GameSession. Current score: {currentScore}, Max height: {maxHeightReached:F2}");
        }
    }
    
    public void EndGame()
    {
        if (!gameActive) return;
        
        gameActive = false;
        float sessionTime = Time.time - sessionStartTime;
        
        if (HighscoreManager.Instance != null)
        {
            HighscoreManager.Instance.AddPlayTime(sessionTime);
        }
        
        OnGameEnd?.Invoke();
        
        Debug.Log($"Game ended! Final score: {currentScore}");
    }
    
    void UpdateScore()
    {
        if (playerTransform != null)
        {
            float currentHeight = playerTransform.position.y;
            
            // Update max height reached
            if (currentHeight > maxHeightReached)
            {
                maxHeightReached = currentHeight;
            }
            
            // Calculate height difference from starting point
            float heightFromStart = maxHeightReached - startingHeight;
            
            // Apply minimum height requirement
            float scoringHeight = Mathf.Max(0, heightFromStart - minimumScoreHeight);
            
            // Calculate new score based on height
            int newScore = Mathf.FloorToInt(scoringHeight * heightMultiplier);
            
            // Update score if it changed
            if (newScore != currentScore)
            {
                currentScore = newScore;
                OnScoreChanged?.Invoke(currentScore);
                UpdateScoreDisplay();
                
                if (showHeightDebug)
                {
                    Debug.Log($"Height Score Update - Current: {currentHeight:F2}, Max: {maxHeightReached:F2}, " +
                             $"Height from start: {heightFromStart:F2}, Score: {currentScore}");
                }
            }
        }
    }
    
    void CheckGameEnd()
    {
        if (playerTransform != null)
        {
            if (playerTransform.position.y < -10f)
            {
                EndGame();
            }
        }
    }
    
    public void AddScore(int points)
    {
        if (!gameActive) return;
        
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        UpdateScoreDisplay();
    }
    
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            if (showHeightDebug && playerTransform != null)
            {
                float currentHeight = playerTransform.position.y;
                float heightFromStart = maxHeightReached - startingHeight;
                scoreText.text = $"Score: {currentScore}\nHeight: {heightFromStart:F1}m\nMax: {maxHeightReached:F1}m";
            }
            else
            {
                scoreText.text = $"Score: {currentScore}";
            }
        }
    }
    
    void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverScoreText != null)
                gameOverScoreText.text = $"Final Score: {currentScore}";
                
            if (HighscoreManager.Instance != null)
            {
                int personalBest = HighscoreManager.Instance.GetPersonalBest();
                if (currentScore > personalBest)
                {
                    if (gameOverScoreText != null)
                        gameOverScoreText.text += "\nNEW PERSONAL BEST!";
                }
                
                int rank = HighscoreManager.Instance.GetPlayerRank(currentScore);
                if (gameOverScoreText != null)
                    gameOverScoreText.text += $"\nRank: #{rank}";
            }
        }
    }
    
    public void SubmitCurrentScore()
    {
        if (scoreSubmitted || HighscoreManager.Instance == null) return;
        
        string playerName = "Player";
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
            playerName = playerNameInput.text;
            
        HighscoreManager.Instance.SubmitScore(playerName, currentScore);
        scoreSubmitted = true;
        
        if (submitScoreButton != null)
        {
            submitScoreButton.interactable = false;
            Text buttonText = submitScoreButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = "Score Submitted";
        }
    }
    
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && gameActive)
        {
            float sessionTime = Time.time - sessionStartTime;
            if (HighscoreManager.Instance != null)
            {
                HighscoreManager.Instance.AddPlayTime(sessionTime);
            }
        }
        else if (!pauseStatus)
        {
            sessionStartTime = Time.time;
        }
    }
} 