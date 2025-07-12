using UnityEngine;
using UnityEngine.UI;

public class NameInputPopup : MonoBehaviour
{
    [Header("Popup UI")]
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private CanvasGroup nameInputCanvasGroup;
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private Button submitNameButton;
    [SerializeField] private Button skipButton;
    
    [Header("UI Text")]
    [SerializeField] private Text promptText;
    [SerializeField] private Text scoreText;
    
    [Header("Animation Settings")]
    [SerializeField] private bool useAnimations = true;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float delayBeforeShow = 0.5f;
    
    private int currentScore = 0;
    private bool nameSubmitted = false;
    
    public static NameInputPopup Instance { get; private set; }
    
    // Events
    public event System.Action<string> OnNameSubmitted;
    public event System.Action OnNameSkipped;
    
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
        
        // Initially hide the popup
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);
    }
    
    void SetupUI()
    {
        // Setup canvas group for animations
        if (nameInputCanvasGroup == null && nameInputPanel != null)
        {
            nameInputCanvasGroup = nameInputPanel.GetComponent<CanvasGroup>();
            if (nameInputCanvasGroup == null)
            {
                nameInputCanvasGroup = nameInputPanel.AddComponent<CanvasGroup>();
            }
        }
        
        // Setup buttons
        if (submitNameButton != null)
            submitNameButton.onClick.AddListener(SubmitName);
            
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipName);
            
        // Setup input field
        if (playerNameInput != null)
        {
            playerNameInput.onEndEdit.AddListener(OnInputEndEdit);
            playerNameInput.characterLimit = 20; // Limit name length
        }
        
        // Setup default text
        if (promptText != null)
            promptText.text = "Game over!";
    }
    
    public void ShowNameInput(int score)
    {
        if (nameInputPanel == null) return;
        
        currentScore = score;
        nameSubmitted = false;
        
        // Update score display
        if (scoreText != null)
            scoreText.text = $"Your Score: {score}";
        
        // Clear previous input
        if (playerNameInput != null)
        {
            playerNameInput.text = "";
            playerNameInput.Select();
            playerNameInput.ActivateInputField();
        }
        
        // Show the panel
        nameInputPanel.SetActive(true);
        
        // Show with animation or immediately
        if (useAnimations)
        {
            StartCoroutine(ShowWithDelay());
        }
        else
        {
            if (nameInputCanvasGroup != null)
                nameInputCanvasGroup.alpha = 1f;
        }
    }
    
    public void HideNameInput()
    {
        if (useAnimations && nameInputCanvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            if (nameInputPanel != null)
                nameInputPanel.SetActive(false);
        }
    }
    
    void SubmitName()
    {
        if (nameSubmitted) return;
        
        string playerName = "Player";
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text.Trim()))
        {
            playerName = playerNameInput.text.Trim();
        }
        
        nameSubmitted = true;
        
        // Submit score to HighscoreManager
        if (HighscoreManager.Instance != null)
        {
            HighscoreManager.Instance.SubmitScore(playerName, currentScore);
        }
        
        // Notify listeners
        OnNameSubmitted?.Invoke(playerName);
        
        // Hide popup
        HideNameInput();
        
        Debug.Log($"Name submitted: {playerName} with score {currentScore}");
    }
    
    void SkipName()
    {
        if (nameSubmitted) return;
        
        nameSubmitted = true;
        
        // Submit with default name
        if (HighscoreManager.Instance != null)
        {
            HighscoreManager.Instance.SubmitScore("Anonymous", currentScore);
        }
        
        // Notify listeners
        OnNameSkipped?.Invoke();
        
        // Hide popup
        HideNameInput();
        
        Debug.Log($"Name skipped - submitted as Anonymous with score {currentScore}");
    }
    
    void OnInputEndEdit(string input)
    {
        // Submit when Enter is pressed
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitName();
        }
    }
    
    System.Collections.IEnumerator ShowWithDelay()
    {
        if (nameInputCanvasGroup != null)
            nameInputCanvasGroup.alpha = 0f;
        
        // Wait for delay
        yield return new WaitForSeconds(delayBeforeShow);
        
        // Fade in
        yield return StartCoroutine(FadeIn());
    }
    
    System.Collections.IEnumerator FadeIn()
    {
        if (nameInputCanvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            nameInputCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            yield return null;
        }
        
        nameInputCanvasGroup.alpha = 1f;
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        if (nameInputCanvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            nameInputCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeInDuration));
            yield return null;
        }
        
        nameInputCanvasGroup.alpha = 0f;
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);
    }
} 