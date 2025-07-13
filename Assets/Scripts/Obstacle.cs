using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float fallSpeed;
    Rigidbody2D rb;
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocityY = -fallSpeed * Time.deltaTime;
    }
    void Update()
    {
        if (transform.position.y < Camera.main.ViewportToWorldPoint(Vector3.zero).y - 1f)
        {
            FindAnyObjectByType<ObstacleSpawn>().ReturnToPool(gameObject);
        }
    }

}
