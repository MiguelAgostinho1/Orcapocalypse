using UnityEngine;
using System.Collections;

public class YachtDestroyController : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private Sprite _wreckedSprite;

    private YachtMovement _movement;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private bool _isSinking = false;

    public bool IsSinking => _isSinking;

    private void Awake()
    {
        _movement = GetComponent<YachtMovement>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
    }

    public void StartSinking()
    {
        if (_isSinking) return;
        StartCoroutine(SinkingSequence());
    }

    private IEnumerator SinkingSequence()
    {
        _isSinking = true;

        if (_movement != null) _movement.enabled = false;
        if (_wreckedSprite != null) _spriteRenderer.sprite = _wreckedSprite;
        if (_collider != null) _collider.isTrigger = true;

        Vector3 startPos = transform.position;
        float elapsed = 0f;

        _rb.bodyType = RigidbodyType2D.Kinematic;

        // This loop can run "forever" since Update will destroy the object
        while (true)
        {
            elapsed += Time.deltaTime;

            float descent = elapsed * 0.5f;

            float bob = Mathf.Sin(Time.time * 1.2f) * 0.04f;

            // Capsizing until 60 degrees
            float tilt = Mathf.MoveTowards(transform.rotation.eulerAngles.z, 60f, Time.deltaTime * 2f);

            // Sinking transform
            transform.SetPositionAndRotation(new Vector3(startPos.x, startPos.y - descent + bob, startPos.z), Quaternion.Euler(0, 0, tilt));

            // Darken as it goes deeper
            float depthPercent = Mathf.Clamp01(descent / 5f);
            _spriteRenderer.color = Color.Lerp(Color.white, new Color(0.2f, 0.2f, 0.2f, 1f), depthPercent);

            yield return null;
        }
    }

    private void Update()
    {
        CheckIfOffScreen();
    }

    private void CheckIfOffScreen()
    {
        // Check if the top of the boat is well below the camera view
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPos.y < -0.2f)
        {
            Debug.Log("DESTROYING YACHT");
            Destroy(gameObject);
        }
    }
}
