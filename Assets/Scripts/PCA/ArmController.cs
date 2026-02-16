using UnityEngine;

public class ArmController : MonoBehaviour
{
    #region
    [Space, Header("Settings")]
    [SerializeField] float stepRange = 5.5f;
    [SerializeField] int segmentAmount = 3;
    [Space, Header("Components")]
    [SerializeField] GameObject anchorObject;
    [SerializeField] GameObject endObject;
    [SerializeField] Material material;

    private Node[] nodes;
    private LineRenderer lineRenderer;
    private Vector2 _targetPosition = new Vector2(100, 100);

    #endregion

    private void OnEnable()
    {
        InitializeRenderer();
        InitializeNodes();
    }

    private void InitializeRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = segmentAmount;
        lineRenderer.material = material;
    }

    private void InitializeNodes()
    {
        nodes = new Node[segmentAmount];
        for (int i = 0; i < segmentAmount; i++)
        {
            nodes[i] = new Node(
                Vector2.zero,       //Position
                2                   //Radius
            );
        }
    }

    void Update()
    {
        var anchorPosition = anchorObject.transform.position;

        if (Vector2.Distance(anchorPosition, _targetPosition) > stepRange)
        {
            _targetPosition = endObject.transform.position;
        }

        IkSolver(_targetPosition, anchorPosition);
        DrawLines();
    }

    private void DrawLines()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            lineRenderer.SetPosition(i, node.Position);
        }

    }

    private void IkSolver(Vector2 target, Vector2 anchor)
    {
        for (int i = 0; i < 10; i++)
        {
            //set first to target
            nodes[0].Position = target;
            //Loop forward through list and update all nodes to correct pos
            for (int ii = 1; ii < nodes.Length; ii++)
            {
                nodes[ii].SetPosition(nodes[ii - 1]);
            }

            //set last to anchor
            nodes[^1].Position = anchor;
            //Loop forward through list and update all nodes to correct pos
            for (int iii = nodes.Length - 2; iii >= 0; iii--)
            {
                nodes[iii].SetPosition(nodes[iii + 1]);
            }
        }
    }
}
