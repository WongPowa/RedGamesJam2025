using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;
    
    [Header("Looping Settings")]
    public bool shouldLoop = true;
    
    [Header("Color Darkening (Sky Layer)")]
    public bool shouldDarken = false;
    public Color startColor = Color.white;
    public Color endColor = Color.black;
    public float darkeningStartHeight = 50f;
    public float darkeningEndHeight = 150f;
    public bool resetToStartColor = true;
    
    private Transform cameraTransform;
    private Vector3 previousCameraPosition;
    private float spriteHeight;
    private Transform[] backgrounds;
    private SpriteRenderer[] spriteRenderers;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        previousCameraPosition = cameraTransform.position;

        backgrounds = new Transform[transform.childCount];
        spriteRenderers = new SpriteRenderer[transform.childCount];
        
        for (int i = 0; i < transform.childCount; i++)
        {
            backgrounds[i] = transform.GetChild(i);
            spriteRenderers[i] = backgrounds[i].GetComponent<SpriteRenderer>();
        }

        spriteHeight = backgrounds[0].GetComponent<SpriteRenderer>().bounds.size.y;
        
        if (shouldDarken)
        {
            if (resetToStartColor)
            {
                foreach (SpriteRenderer sr in spriteRenderers)
                {
                    if (sr != null)
                    {
                        sr.color = startColor;
                    }
                }
            }
        }
    }

    void Update()
    {
        Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, deltaMovement.y * parallaxFactor, 0);
        previousCameraPosition = cameraTransform.position;

        if (shouldLoop)
        {
            foreach (Transform bg in backgrounds)
            {
                float camBottomEdge = cameraTransform.position.y - Camera.main.orthographicSize;
                float camTopEdge = cameraTransform.position.y + Camera.main.orthographicSize;
                float bgTopEdge = bg.position.y + spriteHeight / 2;
                float bgBottomEdge = bg.position.y - spriteHeight / 2;

                if (bgTopEdge < camBottomEdge)
                {
                    float topmostY = GetTopmostBackgroundY();
                    bg.position = new Vector3(bg.position.x, topmostY + spriteHeight, bg.position.z);
                }
                else if (bgBottomEdge > camTopEdge)
                {
                    float bottommostY = GetBottommostBackgroundY();
                    bg.position = new Vector3(bg.position.x, bottommostY - spriteHeight, bg.position.z);
                }
            }
        }
        
        if (shouldDarken)
        {
            UpdateSkyColor();
        }
    }

    float GetTopmostBackgroundY()
    {
        float maxY = backgrounds[0].position.y;
        foreach (Transform bg in backgrounds)
        {
            if (bg.position.y > maxY)
                maxY = bg.position.y;
        }
        return maxY;
    }

    float GetBottommostBackgroundY()
    {
        float minY = backgrounds[0].position.y;
        foreach (Transform bg in backgrounds)
        {
            if (bg.position.y < minY)
                minY = bg.position.y;
        }
        return minY;
    }
    
    void UpdateSkyColor()
    {
        Color currentColor;
        float currentHeight = cameraTransform.position.y;
        
        if (currentHeight <= darkeningStartHeight)
        {
            currentColor = startColor;
        }
        else if (currentHeight >= darkeningEndHeight)
        {
            currentColor = endColor;
        }
        else
        {
            float heightProgress = (currentHeight - darkeningStartHeight) / (darkeningEndHeight - darkeningStartHeight);
            currentColor = Color.Lerp(startColor, endColor, heightProgress);
        }
        
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.color = currentColor;
            }
        }
    }
} 