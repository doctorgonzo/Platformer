using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour
{
   [SerializeField] private int health = 3;
   [SerializeField] private int maxHealth = 3;
   [SerializeField] private int lives = 3;
   [SerializeField] private int maxLives = 3;
   [SerializeField] private GameObject explosionPrefab;
   private bool isDead;
   public static Player Instance { get; private set; }
   [Header("Cached Components")]
   [SerializeField] private Image[] livesUI;
   [SerializeField] private TMPro.TextMeshProUGUI scoreText;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }   

   public void TakeDamage(int damage)
   {
      if (isDead)
      {
         return; // can't die twice while waiting to respawn
      }
      health -= damage;
      switch(health)
      {
         case 2:
            PlayerMovement.Instance.gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
            break;
         case 1:
            PlayerMovement.Instance.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            break;
      }
      if (health <= 0)
      {
        lives--;
        livesUI[lives].enabled = false; // Update UI to reflect lost life
        Die();
        Debug.Log("Player Died. Lives left: " + lives);
      }
   }

    public void Respawn()
    {
        PlayerMovement.Instance.rb.linearVelocity = Vector2.zero; // Reset velocity on respawn
        gameObject.transform.position = PlayerMovement.Instance.spawnPoint.transform.position;
        SetPlayerActive(true); // re-enable after the move so there's no contact at the death spot
        PlayerMovement.Instance.GetComponent<SpriteRenderer>().color = Color.green; // Reset color on respawn
    }

    private IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(1.5f);
        Respawn();
    }

    public void Die()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, new Vector2(transform.position.x, transform.position.y + 1), Quaternion.identity);
            explosion.GetComponent<Explosion>().SetColor(PlayerMovement.Instance.GetComponent<SpriteRenderer>().color);
        }
        SetPlayerActive(false); // hidden and inert while "exploded"
        if (lives > 0)
        {
            health = maxHealth;
            StartCoroutine(RespawnTimer());
        }
        else
        {
            // Game over logic here
            Debug.Log("Game Over");
        }
    }

    private void SetPlayerActive(bool active)
    {
        PlayerMovement movement = PlayerMovement.Instance;
        movement.enabled = active;                        // no input/steering
        movement.rb.simulated = active;                   // no physics, gravity, or contacts
        movement.GetComponent<SpriteRenderer>().enabled = active;
        isDead = !active;
    }
}
