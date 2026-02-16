using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ArmController : MonoBehaviour
{
    [SerializeField] List<Node> nodes;
    [SerializeField] GameObject anchorObject;
    [SerializeField] GameObject endObject;
    private Vector2 anchorPosition;
    private Vector2 _targetPosition = new Vector2(100, 100);

    private Node _root;
    private Node _endEffector;

    // What do i Need to add?
    // I need the arm to create a target point.
    // I need the root to be ancered.

    // Update is called once per frame
    void Update()
    {
        anchorPosition = anchorObject.transform.position;
        //_targetPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Vector2.Distance(anchorPosition, _targetPosition) > 5)
        {
            _targetPosition = endObject.transform.position;
        }
        Debug.DrawLine(anchorObject.transform.position, _targetPosition, Color.red);

        IkSolver(_targetPosition, anchorPosition);
    }

    ///FABRIK
    ///1. Place end effector at target
    ///2. Adjust joints backwards so that length constraint is satisfied
    ///3. Place root back at origin
    ///4. Adjust joints forwards so that length constraint is satisfied
    ///5. Repeat until reached or max iterations
    private void IkSolver(Vector2 target, Vector2 anchor)
    {
        for (int i = 0; i < 10; i++)
        {
            //set first to target
            nodes[0]._position = target;
            //Loop forward through list and update all nodes to correct pos
            for (int ii = 1; ii < nodes.Count; ii++)
            {
                nodes[ii].SetPosition(nodes[ii - 1]);
            }

            //set last to anchor
            nodes[nodes.Count - 1]._position = anchor;
            //Loop forward through list and update all nodes to correct pos
            for (int iii = nodes.Count - 2; iii >= 0; iii--)
            {
                nodes[iii].SetPosition(nodes[iii + 1]);
            }
        }

        foreach (var node in nodes)
        {
            node.transform.position = node._position;
        }

    }
}
