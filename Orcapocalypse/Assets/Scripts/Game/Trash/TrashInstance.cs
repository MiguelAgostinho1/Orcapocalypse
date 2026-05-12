using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class TrashInstance : MonoBehaviour
{
    private BoxCollider2D _collider;
    private SpriteRenderer _sr;
    private TrashData _data;

    public void Initialize(TrashData data)
    {
        _data = data;
        _sr = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();

        // Set the visuals
        _sr.sprite = _data.visualSprite;
        _sr.sortingOrder = 5; // Ensure it's behind/in front of the Orca as desired

        // Shrink the object
        transform.localScale = Vector3.one * _data.baseScale;

        // AUTO-ADJUST HITBOX
        // This makes the box match the exact dimensions of the sprite
        _collider.size = _sr.sprite.bounds.size;
        _collider.isTrigger = true;
    }

    void Update()
    {
        // Simple sinking
        transform.Translate(Vector2.down * _data.sinkSpeed * Time.deltaTime, Space.World);

        // Tumble effect
        transform.Rotate(0, 0, _data.rotationSpeed * Time.deltaTime);

        // Despawn Logic when Off-Screen
        CheckIfOffScreen();
    }

    private void CheckIfOffScreen()
    {
        // Check if the top of the boat is well below the camera view
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPos.y < -0.2f)
        {
            Debug.Log("DESTROYING TRASH");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure your Orca GameObject has the "Player" tag in the Inspector!
        if (other.CompareTag("Player"))
        {
            // Check if the object we hit has a HealthController
            HealthController health = other.GetComponent<HealthController>();

            if (health != null)
            {
                health.TakeDamage(_data.damageAmount);
                other.GetComponent<PlayerMovement>()?.Stun(0.1f);
            }
        }
    }
}