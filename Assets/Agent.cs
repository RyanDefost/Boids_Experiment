using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private float speed = 1;

    [SerializeField] private float cohesionFactor = 0.0005f;
    [SerializeField] private float avoidanceFactor = 0.05f;
    [SerializeField] private float AlignmentFactor = 0.05f;

    private Vector2 velocity = Vector2.zero;

    private AgentManager agentManager;
    private SpriteRenderer spriteRenderer;

    private Vector2 averageDiraction = Vector3.zero;
    private int neighbourCount = 0;

    private Vector2 outerBounds = new Vector2(40, 20);
    private Vector2 randomDiraction = Vector2.zero;

    public Color TeamColor { get; private set; }
    public Vector2 GridPosition { get; set; }

    public void OnEnable()
    {
        this.agentManager = FindObjectOfType<AgentManager>();
        this.spriteRenderer = GetComponent<SpriteRenderer>();

        //
        var gridPositionX = Mathf.Floor(transform.position.x / agentManager.gridSize);
        var gridPositionY = Mathf.Floor(transform.position.y / agentManager.gridSize);
        GridPosition = new Vector2(gridPositionX, gridPositionY);

        //
        if (transform.position.x > 0) TeamColor = Color.black;
        else TeamColor = Color.white;

        this.spriteRenderer.color = TeamColor;

        //Set random start direction
        this.randomDiraction = new Vector2(
            Random.Range(-1, 2),
            Random.Range(-1, 2)
        );
        if (randomDiraction == Vector2.zero) // FIX
        {
            randomDiraction = Vector2.one;
        }
        velocity = randomDiraction * 5;
    }

    public void UpdateAgent()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            velocity += GetMouseDiraction();
        }
        if (Input.GetKey(KeyCode.Mouse1))
        {
            velocity -= GetMouseDiraction();
        }

        ActorCohesion();
        ActorSeparation();
        ActorAlignment();

        LimitSpeed();

        CheckBounds();

        transform.position += (Vector3)this.velocity * this.speed * Time.deltaTime;
        Debug.DrawLine(transform.position, transform.position + (Vector3)velocity, Color.red);
        SetRotation();

        GridPosition = new Vector2(
            Mathf.Floor(transform.position.x / agentManager.gridSize),
            Mathf.Floor(transform.position.y / agentManager.gridSize)
        );
    }

    private void SetRotation()
    {
        Vector3 diff = (transform.position + (Vector3)velocity) - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }

    private void ActorCohesion()
    {
        var maxDistance = 5;

        neighbourCount = 0;
        averageDiraction = Vector2.zero;
        foreach (var actor in agentManager.GetAgents().Keys)
        {
            if (actor.GridPosition != GridPosition) continue;

            var currentDistance = Vector2.Distance(this.transform.position, actor.transform.position);
            if (currentDistance < maxDistance)
            {
                averageDiraction += (Vector2)actor.transform.position;
                neighbourCount++;
            }
        }

        if (neighbourCount > 0)
        {
            averageDiraction = averageDiraction / neighbourCount;
            this.velocity.x += (averageDiraction.x - this.transform.position.x) * cohesionFactor;
            this.velocity.y += (averageDiraction.y - this.transform.position.y) * cohesionFactor;
        }

    }

    private void ActorSeparation()
    {
        var minDistance = 1;
        var maxDistance = 5;

        Vector3 closePos = Vector3.zero;
        foreach (var actor in agentManager.GetAgents().Keys)
        {
            if (actor.GridPosition != GridPosition) continue;

            var currentDistance = Vector2.Distance(this.transform.position, actor.transform.position);
            if (currentDistance <= maxDistance && TeamColor != actor.TeamColor)
            {
                closePos += this.transform.position - actor.transform.position;
            }
            else if (currentDistance <= minDistance)
            {
                closePos += this.transform.position - actor.transform.position;
            }
        }

        this.velocity += (Vector2)closePos * avoidanceFactor;
    }

    private void ActorAlignment()
    {
        var maxDistance = 5;

        neighbourCount = 0;
        averageDiraction = Vector2.zero;
        foreach (var actor in agentManager.GetAgents().Keys)
        {
            if (actor.GridPosition != GridPosition) continue;

            var currentDistance = Vector2.Distance(this.transform.position, actor.transform.position);
            if (currentDistance < maxDistance)
            {
                if (TeamColor != actor.TeamColor) continue;

                averageDiraction += actor.velocity;
                neighbourCount++;
            }
        }

        if (neighbourCount > 0)
        {
            averageDiraction = averageDiraction / neighbourCount;
            this.velocity += (averageDiraction - velocity) * AlignmentFactor;
        }
    }

    private void LimitSpeed()
    {
        var speedLimit = 10;
        var speedMin = 1;

        var speed = Mathf.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y);
        if (speed > speedLimit)
        {
            velocity = (velocity / speed) * speedLimit;
        }
        else if (speed < speedMin)
        {
            velocity = (velocity / speed) * speedMin;
        }

    }

    private void CheckBounds()
    {
        float turnFactor = 0.5f;

        Vector2 agentPosition = this.transform.position;
        if (agentPosition.x < outerBounds.x)
        {
            velocity.x += turnFactor;
        }
        if (agentPosition.x > -outerBounds.x)
        {
            velocity.x -= turnFactor;
        }
        if (agentPosition.y < outerBounds.y)
        {
            velocity.y += turnFactor;
        }
        if (agentPosition.y > -outerBounds.y)
        {
            velocity.y -= turnFactor;
        }
    }

    private Vector2 GetMouseDiraction()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = (worldPosition - transform.position).normalized;

        return direction;
    }
}
