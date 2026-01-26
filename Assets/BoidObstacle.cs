using UnityEngine;

public class BoidObstacle : MonoBehaviour
{
    public Collider2D Collider;

    private void Awake()
    {
        Collider = GetComponent<Collider2D>();
    }
}
