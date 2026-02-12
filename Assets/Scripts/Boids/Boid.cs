using UnityEngine;

public struct Boid
{
    public Vector2 Position;

    public GameObject GameObject;
    public SpriteRenderer Renderer;

    public Vector2 GridPosition;

    public Vector2 Velocity;
    public float Speed;

    public Color TeamColor;

    public Boid(Vector2 position, float gridSize, Color teamColor, Sprite sprite, Vector2 velocity)
    {
        GameObject = new GameObject();
        Renderer = GameObject.AddComponent<SpriteRenderer>();
        Renderer.sprite = sprite;

        GameObject.transform.position = position;
        Position = position;

        TeamColor = teamColor;
        Renderer.color = teamColor;

        GridPosition = new Vector2(
            Mathf.Floor(position.x / gridSize),
            Mathf.Floor(position.y / gridSize)
        );

        Velocity = velocity;
        Speed = 10;
    }
}
