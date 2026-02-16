using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Vector2 _position = Vector2.zero;
    private float _radius = 0.5f;

    //public Node ConnectedNode;


    // Start is called before the first frame update
    void Start()
    {
        _position = transform.position;
    }

    public void SetPosition(Node ConnectedNode)
    {
        var diraction = new Vector2(
            _position.x - ConnectedNode._position.x,
            _position.y - ConnectedNode._position.y
            ).normalized;

        var ConnectionPosition = diraction * _radius;
        _position = ConnectionPosition + (Vector2)ConnectedNode._position;
    }
}
