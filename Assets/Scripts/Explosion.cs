using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float force = 10f;
    [SerializeField] private float torque = 20f;
    [SerializeField] private float lifetime = 3f;

    private void Start()
    {
        foreach (Rigidbody2D piece in GetComponentsInChildren<Rigidbody2D>())
        {
            Vector2 direction = ((Vector2)piece.transform.position - (Vector2)transform.position).normalized;
            if (direction == Vector2.zero)
            {
                direction = Random.insideUnitCircle.normalized; // piece exactly at center
            }
            // Impulse, not Force: a one-off velocity change, applied over a single step
            piece.AddForce(direction * force * Random.Range(0.8f, 1.2f), ForceMode2D.Impulse);
            piece.AddTorque(Random.Range(-torque, torque), ForceMode2D.Impulse);
        }

        Destroy(gameObject, lifetime);
    }

    public void SetColor(Color color)
    {
        foreach (SpriteRenderer piece in GetComponentsInChildren<SpriteRenderer>())
        {
            piece.color = color;
        }
    }
}
