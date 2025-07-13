using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    [Header("Flow Settings")]
    [SerializeField] private bool skipNameInput = false; // For testing
    
    public static GameFlowController Instance { get; private set; }
    
    private int currentScore = 0;
    private string currentPlayerName = "";
    
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
        SetupEventListeners();
    }
    
    void SetupEventListeners()
    {
        // Listen to GameSession events
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnGameEnd += HandleGameEnd;
            GameSession.Instance.OnGameStart += HandleGameStart;
        }
        
        // Listen to NameInputPopup events
        if (NameInputPopup.Instance != null)
        {
            NameInputPopup.Instance.OnNameSubmitted += HandleNameSubmitted;
            NameInputPopup.Instance.OnNameSkipped += HandleNameSkipped;
        }
    }
    
    void HandleGameEnd()
    {
        if (GameSession.Instance == null) return;
        
        currentScore = GameSession.Instance.CurrentScore;
        
        Debug.Log($"Game ended with score: {currentScore}");
        
        // Step 1: Fade to black, then show name input popup
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeInBlack(0.7f, () => {
                if (skipNameInput)
                {
                    HandleNameSkipped();
                }
                else
                {
                    ShowNameInputPopup();
                }
            });
        }
        else
        {
            // Fallback: no fade
            if (skipNameInput)
            {
                HandleNameSkipped();
            }
            else
            {
                ShowNameInputPopup();
            }
        }
    }
    
    void HandleGameStart()
    {
        // Hide any open screens when game starts
        HideAllScreens();
        
        Debug.Log("Game started - hiding all screens");
    }
    
    void ShowNameInputPopup()
    {
        if (NameInputPopup.Instance != null)
        {
            NameInputPopup.Instance.ShowNameInput(currentScore);
            Debug.Log("Showing name input popup");
        }
        else
        {
            Debug.LogWarning("NameInputPopup not found - skipping to game over screen");
            HandleNameSkipped();
        }
    }
    
    void HandleNameSubmitted(string playerName)
    {
        currentPlayerName = playerName;
        
        Debug.Log($"Name submitted: {playerName}");
        
        // Step 2: Show game over screen with leaderboard
        ShowGameOverScreen();
    }
    
    void HandleNameSkipped()
    {
        currentPlayerName = "Anonymous";
        
        Debug.Log("Name skipped - using Anonymous");
        
        // Step 2: Show game over screen with leaderboard
        ShowGameOverScreen();
    }
    
    void ShowGameOverScreen()
    {
        if (GameOverScreen.Instance != null)
        {
            GameOverScreen.Instance.ShowGameOverScreen(currentScore, currentPlayerName);
            Debug.Log($"Showing game over screen for {currentPlayerName} with score {currentScore}");
        }
        else
        {
            Debug.LogWarning("GameOverScreen not found");
        }
    }
    
    void HideAllScreens()
    {
        // Hide name input popup
        if (NameInputPopup.Instance != null)
        {
            NameInputPopup.Instance.HideNameInput();
        }
        
        // Hide game over screen
        if (GameOverScreen.Instance != null)
        {
            GameOverScreen.Instance.HideGameOverScreen();
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnGameEnd -= HandleGameEnd;
            GameSession.Instance.OnGameStart -= HandleGameStart;
        }
        
        if (NameInputPopup.Instance != null)
        {
            NameInputPopup.Instance.OnNameSubmitted -= HandleNameSubmitted;
            NameInputPopup.Instance.OnNameSkipped -= HandleNameSkipped;
        }
    }
    
    // Public methods for testing
    public void TestNameInput()
    {
        ShowNameInputPopup();
    }
    
    public void TestGameOverScreen()
    {
        ShowGameOverScreen();
    }
} 