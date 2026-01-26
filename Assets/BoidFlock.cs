using UnityEngine;

public struct Boid
{
    public Vector2 Position;
    public GameObject gameObject;
    public SpriteRenderer renderer;

    public Vector2 GridPosition;

    public Vector2 Velocity;
    public float Speed;

    public Color TeamColor;

    public Boid(Vector2 position, Vector2 gridPosition, Color teamColor, Sprite sprite, Vector2 velocity)
    {
        this.gameObject = new GameObject();
        this.renderer = this.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        this.gameObject.transform.position = position;
        this.Position = position;

        this.TeamColor = teamColor;
        renderer.color = this.TeamColor;

        this.GridPosition = gridPosition;

        this.Velocity = velocity;
        this.Speed = 10;
    }
}

public class BoidFlock : MonoBehaviour
{
    [SerializeField] private int agentAmount = 0;
    [SerializeField] private Vector2 agentArea = new Vector2(40, 20);
    public int gridSize = 6;

    private Boid[] agents;
    private Boid currentBoid;

    private BoidObstacle[] obstacles;

    //
    [SerializeField] private float cohesionFactor = 0.0005f;
    [SerializeField] private float avoidanceFactor = 0.05f;
    [SerializeField] private float AlignmentFactor = 0.05f;
    [SerializeField] private float turnFactor = 0.1f;

    [Space]
    [SerializeField] private int minDistance = 1;
    [SerializeField] private int maxDistance = 5;

    [Space]
    [SerializeField] private Sprite sprite;

    private int NeighbourCount = 0;
    private Vector2 averageCenter = Vector2.zero;
    private Vector2 averageDiraction = Vector2.zero;
    private Vector2 closePos = Vector2.zero;

    private Vector2 outerBounds = new Vector2(40, 20);


    private void OnEnable()
    {
        agents = new Boid[agentAmount];
        CreateAgent(this.agentAmount);

        obstacles = FindObjectsOfType<BoidObstacle>();
    }

    // Update is called once per frame
    private void Update()
    {
        for (int i = 0; i < agents.Length; i++)
        {
            currentBoid = agents[i];

            BoidLogic();
            LimitSpeed();
            CheckBounds();
            AvoidObstacles();

            currentBoid.gameObject.transform.position += (Vector3)currentBoid.Velocity * currentBoid.Speed * Time.deltaTime;
            currentBoid.Position = currentBoid.gameObject.transform.position;

            SetRotation();

            currentBoid.GridPosition = new Vector2(
                Mathf.Floor(currentBoid.Position.x / gridSize),
                Mathf.Floor(currentBoid.Position.y / gridSize)
            );

            agents[i] = currentBoid; //Saves the current values.
        }
    }

    private void CreateAgent(int amount)
    {
        if (amount <= 0) return;

        for (int i = 0; i < amount; i++)
        {
            //Set random start direction
            var randomDiraction = new Vector2(
                Random.Range(-1, 2),
                Random.Range(-1, 2)
            );
            if (randomDiraction == Vector2.zero) // FIX
            {
                randomDiraction = Vector2.one;
            }

            var randomPosition = new Vector3(
                Random.Range(-agentArea.x, agentArea.x),
                Random.Range(-agentArea.y, agentArea.y),
                0
            );

            var gridPositionX = Mathf.Floor(randomPosition.x / gridSize);
            var gridPositionY = Mathf.Floor(randomPosition.y / gridSize);
            var gridPosition = new Vector2(gridPositionX, gridPositionY);

            var currentAgent = new Boid(
                randomPosition,
                gridPosition,
                Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f),
                sprite,
                randomDiraction
            );

            this.agents[i] = currentAgent;
        }
    }

    private void SetRotation()
    {
        Vector3 diff = (currentBoid.Position + currentBoid.Velocity) - currentBoid.Position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        currentBoid.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }

    private void BoidLogic()
    {
        NeighbourCount = 0;
        averageCenter = Vector2.zero;
        averageDiraction = Vector2.zero;
        closePos = Vector2.zero;

        for (int i = 0; i < this.agents.Length; i++)
        {
            var agent = this.agents[i];

            if (agent.GridPosition != currentBoid.GridPosition) continue;

            var currentDistance = Vector2.Distance(currentBoid.Position, agent.Position);

            //AVOIDANCE OTHER
            if (currentDistance <= minDistance && currentBoid.TeamColor != agent.TeamColor)
            {
                closePos += (currentBoid.Position - agent.Position);

                currentBoid.TeamColor = agent.TeamColor;
                currentBoid.renderer.color = currentBoid.TeamColor;

                continue;
            }

            if (currentDistance < maxDistance)
            {
                //COHESION
                averageCenter += agent.Position;
                //ALIGNMNENT
                averageDiraction += agent.Velocity;

                NeighbourCount++;
            }

            //SEPERATION
            if (currentDistance <= minDistance)
                closePos += currentBoid.Position - agent.Position;
        }

        if (NeighbourCount > 0)
        {
            //COHESION
            averageCenter = averageCenter / NeighbourCount;
            currentBoid.Velocity.x += (averageCenter.x - currentBoid.Position.x) * cohesionFactor;
            currentBoid.Velocity.y += (averageCenter.y - currentBoid.Position.y) * cohesionFactor;

            //ALIGNMENT
            averageDiraction = averageDiraction / NeighbourCount;
            currentBoid.Velocity += (averageDiraction - currentBoid.Velocity) * AlignmentFactor;
        }

        //SEPERATION
        currentBoid.Velocity += closePos * avoidanceFactor;
    }

    private void AvoidObstacles()
    {
        foreach (var obstacle in obstacles) // BOIDS PASS TROUGH THE RIGHT SIDE.
        {                                   // THEY ALSO ONLY MOVE AWAY AT THE EDGE OF THE OBJECT INSTEAD OF FULLY AVOIDING IT.
            Vector2 nextStep = (Vector3)currentBoid.Velocity * currentBoid.Speed * Time.deltaTime;

            if (obstacle.Collider.bounds.Contains(currentBoid.Position))
            {
                currentBoid.Velocity += (currentBoid.Position - nextStep) * 0.1f;
                print(currentBoid);
            }
        }

    }

    private void LimitSpeed()
    {
        var speedLimit = 1;
        var speedMin = 1;

        var speed = Mathf.Sqrt(currentBoid.Velocity.x * currentBoid.Velocity.x + currentBoid.Velocity.y * currentBoid.Velocity.y);
        if (speed > speedLimit)
        {
            currentBoid.Velocity = (currentBoid.Velocity / speed) * speedLimit;
        }
        else if (speed < speedMin)
        {
            currentBoid.Velocity = (currentBoid.Velocity / speed) * speedMin;
        }
    }

    private void CheckBounds()
    {
        Vector2 agentPosition = currentBoid.Position;

        if (agentPosition.x < outerBounds.x)
        {
            currentBoid.Velocity.x += turnFactor;
        }
        if (agentPosition.x > -outerBounds.x)
        {
            currentBoid.Velocity.x -= turnFactor;
        }
        if (agentPosition.y < outerBounds.y)
        {
            currentBoid.Velocity.y += turnFactor;
        }
        if (agentPosition.y > -outerBounds.y)
        {
            currentBoid.Velocity.y -= turnFactor;
        }
    }
}
