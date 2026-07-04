using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    [SerializeField] private int score = 0;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    
    public static ScoreKeeper Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        scoreText.text = score.ToString();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    public void AddScore(int points)
    {
        score += points;
        scoreText.text = score.ToString();
    }

    public int GetScore()
    {
        return score;
    }
}
