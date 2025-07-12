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
    
    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private Button submitScoreButton;
    [SerializeField] private Button restartButton;
    
    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;
    
    private float startingHeight = 0f;
    private float maxHeightReached = 0f;
    private bool scoreSubmitted = false;
    
    public static GameSession Instance { get; private set; }
    
    public int CurrentScore => currentScore;
    public bool IsGameActive => gameActive;
    
    public event System.Action<int> OnScoreChanged;
    public event System.Action OnGameStart;
    public event System.Action OnGameEnd;
    
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
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
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
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        OnGameStart?.Invoke();
        UpdateScoreDisplay();
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
        
        ShowGameOverScreen();
        OnGameEnd?.Invoke();
    }
    
    void UpdateScore()
    {
        if (playerTransform != null)
        {
            float currentHeight = playerTransform.position.y;
            if (currentHeight > maxHeightReached)
            {
                maxHeightReached = currentHeight;
            }
            
            float heightFromStart = maxHeightReached - startingHeight;
            int newScore = Mathf.FloorToInt(heightFromStart * heightMultiplier);
            
            if (newScore != currentScore)
            {
                currentScore = newScore;
                OnScoreChanged?.Invoke(currentScore);
                UpdateScoreDisplay();
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
            scoreText.text = $"Score: {currentScore}";
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
    
    public void RestartGame()
    {
        StartGame();
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