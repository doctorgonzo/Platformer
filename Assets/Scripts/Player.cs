using UnityEngine;
using UnityEngine.UI;
public class Player : MonoBehaviour
{
   [SerializeField] private int health = 3;
   [SerializeField] private int maxHealth = 3;
   [SerializeField] private int lives = 3;
   [SerializeField] private int maxLives = 3;
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
        PlayerMovement.Instance.GetComponent<SpriteRenderer>().color = Color.green; // Reset color on respawn
    }

    public void Die()
    {
        if (lives > 0)
        {
            health = maxHealth;
            Respawn();
        }
        else
        {
            // Game over logic here
            Debug.Log("Game Over");
        }
    }
}
