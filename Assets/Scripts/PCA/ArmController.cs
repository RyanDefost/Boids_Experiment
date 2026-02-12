using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmController : MonoBehaviour
{
    [SerializeField] List<Node> nodes;

    private void OnEnable()
    {
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Node currentNode = nodes[i];

            if (nodes[i + 1] != null)
            {
                nodes[i + 1].ConnectedNode = currentNode;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];

            if (node == nodes[^1])
                node.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

            node.UpdateNode();
        }
    }
}
