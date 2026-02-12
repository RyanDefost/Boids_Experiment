using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;

public class Node : MonoBehaviour
{
    private Vector2 _position = Vector2.zero;
    private float _radius = 1.5f;

    public Node ConnectedNode;


    // Start is called before the first frame update
    void Start()
    {
        _position = transform.position;
    }

    public void UpdateNode()
    {
        _position = this.transform.position;

        if (ConnectedNode == null) return;

        var diraction = new Vector2(
            ConnectedNode._position.x - this._position.x,
             ConnectedNode._position.y - this._position.y
            ).normalized;

        var ConnectionPosition = diraction * _radius;
        ConnectedNode.transform.position = ConnectionPosition + (Vector2)this.transform.position;
    }
}
