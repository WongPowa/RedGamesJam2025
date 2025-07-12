using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private CanvasGroup blackOverlay;
    [SerializeField] private float defaultFadeDuration = 0.7f;

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
        if (blackOverlay == null)
        {
            // Try to find the overlay in children
            blackOverlay = GetComponentInChildren<CanvasGroup>(true);
        }
        if (blackOverlay != null)
        {
            blackOverlay.alpha = 0f;
            blackOverlay.blocksRaycasts = false;
        }
    }

    public void FadeInBlack(float duration, Action onComplete = null)
    {
        if (blackOverlay == null) return;
        blackOverlay.gameObject.SetActive(true);
        blackOverlay.blocksRaycasts = true;
        StartCoroutine(FadeRoutine(blackOverlay, 0f, 1f, duration > 0 ? duration : defaultFadeDuration, onComplete));
    }

    public void FadeOutBlack(float duration, Action onComplete = null)
    {
        if (blackOverlay == null) return;
        StartCoroutine(FadeRoutine(blackOverlay, 1f, 0f, duration > 0 ? duration : defaultFadeDuration, () => {
            blackOverlay.blocksRaycasts = false;
            blackOverlay.gameObject.SetActive(false);
            onComplete?.Invoke();
        }));
    }

    private IEnumerator FadeRoutine(CanvasGroup group, float from, float to, float duration, Action onComplete)
    {
        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
        onComplete?.Invoke();
    }
} 