using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerDeathController : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverUI;

    private Rigidbody2D _rb;
    private bool _isDead = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // Trigger the "Belly Up" Flip and Death Animation
        StartCoroutine(DeathSequence());
    }

    private void Update()
    {
        // Check if the 'R' key was pressed this frame to restart
        if (_isDead && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartDemo();
        }
    }

    public IEnumerator DeathSequence()
    {
        _isDead = true;

        // Flip Upside Down
        float startRotation = transform.eulerAngles.z;
        float elapsed = 0;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float z = Mathf.Lerp(startRotation, 180f, elapsed / 0.5f);
            transform.rotation = Quaternion.Euler(0, 0, z);
            yield return null;
        }

        // Death Animation
        _rb.linearVelocity = Vector2.up * 5f; // Small jump upwards
        _rb.gravityScale = 1f; // Gravity pulls the orca down naturally

        // Show the Game Over UI after the Death Animation is finished
        if (_gameOverUI != null) _gameOverUI.SetActive(true);
    }

    private void RestartDemo()
    {
        // Reloads the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
