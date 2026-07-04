using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int scoreValue = 100;

    public void OnStomped()
    {
        Debug.Log(name + " was stomped");
        ScoreKeeper.Instance.AddScore(scoreValue);
        Destroy(gameObject);
    }
}
