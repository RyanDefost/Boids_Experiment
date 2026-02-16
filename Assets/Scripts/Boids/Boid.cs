using UnityEngine;

public struct Boid
{
    public Vector2 Position;

    public GameObject bGameObject;
    //public SpriteRenderer Renderer;

    public Vector2 GridPosition;

    public Vector2 Velocity;
    public float Speed;

    public Color TeamColor;

    public Boid(Vector2 position, float gridSize, Color teamColor, GameObject gameObject, Vector2 velocity)
    {
        bGameObject = gameObject;
        //Renderer = bGameObject.AddComponent<SpriteRenderer>();
        //Renderer.sprite = sprite;

        bGameObject.transform.position = position;
        Position = position;

        TeamColor = teamColor;
        //Renderer.color = teamColor;

        GridPosition = new Vector2(
            Mathf.Floor(position.x / gridSize),
            Mathf.Floor(position.y / gridSize)
        );

        Velocity = velocity;
        Speed = 10;
    }
}
