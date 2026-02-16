using UnityEngine;

public struct Node
{
    public Vector2 Position;
    public float Radius;


    public Node(Vector2 position, float radius)
    {
        Position = position;
        Radius = radius;
    }

    public void SetPosition(Node otherNode)
    {
        Vector2 diraction = new Vector2(
            Position.x - otherNode.Position.x,
            Position.y - otherNode.Position.y
        ).normalized;

        Vector2 correctedPosition = diraction * Radius;
        Position = correctedPosition + otherNode.Position;
    }
}
