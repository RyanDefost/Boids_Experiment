using UnityEngine;

public class BoidFlock : MonoBehaviour
{
    [Header("Flock Settings")]
    [SerializeField] private int _boidAmount = 0;

    [SerializeField] private Vector2 _outerBounds = new Vector2(40, 20);
    [SerializeField] private int _gridSize = 6;

    [Space, Header("Boid Settings")]
    [SerializeField] private float _cohesionFactor = 0.0005f;
    [SerializeField] private float _avoidanceFactor = 0.05f;
    [SerializeField] private float _alignmentFactor = 0.05f;
    [SerializeField] private float _turnFactor = 0.1f;

    [Space]
    [SerializeField] private float _maxSpeed = 1;
    [SerializeField] private float _minSpeed = 1;

    [Space]
    [SerializeField] private int _minDistance = 1;
    [SerializeField] private int _maxDistance = 5;

    [Space]
    [SerializeField] private Sprite _sprite;

    private Boid[] _boids;
    private Boid _currentBoid;

    private void OnEnable()
    {
        _boids = new Boid[_boidAmount];
        CreateBoids(_boidAmount);
    }

    private void OnDisable()
    {
        foreach (Boid agent in _boids)
        {
            Destroy(agent.GameObject);
        }
        _boids = null;
    }

    private void Update()
    {
        for (int i = 0; i < _boids.Length; i++)
        {
            _currentBoid = _boids[i];

            BoidLogic();
            LimitSpeed();
            CheckBounds();
            AvoidObstacles();

            _currentBoid.GameObject.transform.position += (Vector3)_currentBoid.Velocity * _currentBoid.Speed * Time.deltaTime;
            _currentBoid.Position = _currentBoid.GameObject.transform.position;

            SetRotation();

            _currentBoid.GridPosition = new Vector2(
                Mathf.Floor(_currentBoid.Position.x / _gridSize),
                Mathf.Floor(_currentBoid.Position.y / _gridSize)
            );

            _boids[i] = _currentBoid;
        }
    }

    /// <summary>
    /// Creates the given amount of boids in the level.
    /// </summary>
    /// <param name="amount">amount of boids that are spawned.</param>
    private void CreateBoids(int amount)
    {
        if (amount <= 0) return;

        for (int i = 0; i < amount; i++)
        {
            // Set random startPosition.
            Vector2 randomPosition = new Vector3(
                Random.Range(-_outerBounds.x, _outerBounds.x),
                Random.Range(-_outerBounds.y, _outerBounds.y),
                0
            );

            // Set random start direction.
            Vector2 randomDiraction = new Vector2(
                Random.Range(-1, 2),
                Random.Range(-1, 2)
            );
            randomDiraction = randomDiraction == Vector2.zero ? Vector2.one : randomDiraction;

            // Set random color.
            Color color = randomPosition.y > 0 ? Color.blue : Color.yellow;

            // CREATE BOID.
            _boids[i] = new Boid(
                randomPosition,
                _gridSize,
                color, //Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f),
                _sprite,
                randomDiraction
            );
        }
    }

    /// <summary>
    /// Rotates the _currentBoid towards its velocity direction.
    /// </summary>
    private void SetRotation() // NOT MY CODE (forgot to note down credits)
    {
        Vector3 diff = (_currentBoid.Position + _currentBoid.Velocity) - _currentBoid.Position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        _currentBoid.GameObject.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }

    /// <summary>
    /// Applies the boid rules to the _currentBoid.
    /// </summary>
    private void BoidLogic()
    {
        var neighbourCount = 0;
        var averageCenter = Vector2.zero;
        var averageDiraction = Vector2.zero;
        var avoidCenter = Vector2.zero;

        for (int i = 0; i < _boids.Length; i++)
        {
            Boid boid = _boids[i];

            if (boid.GridPosition != _currentBoid.GridPosition) continue;

            float currentDistance = Vector2.Distance(_currentBoid.Position, boid.Position);

            //AVOIDANCE OTHER
            if (currentDistance <= _minDistance && _currentBoid.TeamColor != boid.TeamColor)
            {
                avoidCenter += (_currentBoid.Position - boid.Position);

                _currentBoid.TeamColor = boid.TeamColor;
                _currentBoid.Renderer.color = _currentBoid.TeamColor;

                continue;
            }

            if (currentDistance < _maxDistance)
            {
                //COHESION
                averageCenter += boid.Position;
                //ALIGNMNENT
                averageDiraction += boid.Velocity;

                neighbourCount++;
            }

            //SEPERATION
            if (currentDistance <= _minDistance)
                avoidCenter += _currentBoid.Position - boid.Position;
        }

        if (neighbourCount > 0)
        {
            //COHESION
            averageCenter = averageCenter / neighbourCount;
            _currentBoid.Velocity.x += (averageCenter.x - _currentBoid.Position.x) * _cohesionFactor;
            _currentBoid.Velocity.y += (averageCenter.y - _currentBoid.Position.y) * _cohesionFactor;

            //ALIGNMENT
            averageDiraction = averageDiraction / neighbourCount;
            _currentBoid.Velocity += (averageDiraction - _currentBoid.Velocity) * _alignmentFactor;
        }

        //SEPERATION
        _currentBoid.Velocity += avoidCenter * _avoidanceFactor;
    }

    /// <summary>
    /// Avoids the bounds of an obstacle if moving towards it.
    /// </summary>
    private void AvoidObstacles()
    {
        Vector2 diraction = (_currentBoid.Velocity * _currentBoid.Speed * Time.deltaTime).normalized;
        RaycastHit2D hit = Physics2D.Raycast(_currentBoid.Position, diraction, 12);

        if (hit)
        {
            // Fail-save move outside of obstacle.
            if (hit.distance <= 0)
            {
                _currentBoid.Velocity += (_currentBoid.Position - diraction);
                return;
            }

            float turnAvoid = hit.distance < 1 ? 0.9f : 0.4f;
            float roundDirX = Mathf.Round(diraction.x);
            float roundDirY = Mathf.Round(diraction.y);

            // Set avoid direction.
            if (roundDirX <= 0)
            {
                _currentBoid.Velocity.x += turnAvoid;
            }
            else if (roundDirX >= 0)
            {
                _currentBoid.Velocity.x -= turnAvoid;
            }
            if (roundDirY <= 0)
            {
                _currentBoid.Velocity.y += turnAvoid;
            }
            else if (roundDirY >= 0)
            {
                _currentBoid.Velocity.y -= turnAvoid;
            }
        }
    }

    /// <summary>
    /// Sets and limits the amount of velocity based on the min / max speed.
    /// </summary>
    private void LimitSpeed()
    {
        Vector2 velocity = _currentBoid.Velocity;

        float speed = Mathf.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y);
        if (speed > _maxSpeed)
        {
            _currentBoid.Velocity = (velocity / speed) * _maxSpeed;
        }
        else if (speed < _minSpeed)
        {
            _currentBoid.Velocity = (velocity / speed) * _minSpeed;
        }
    }

    /// <summary>
    /// Keeps the _currentBoid inside the given bounds of the level.
    /// </summary>
    private void CheckBounds()
    {
        Vector2 position = _currentBoid.Position;

        if (position.x < _outerBounds.x)
        {
            _currentBoid.Velocity.x += _turnFactor;
        }
        if (position.x > -_outerBounds.x)
        {
            _currentBoid.Velocity.x -= _turnFactor;
        }
        if (position.y < _outerBounds.y)
        {
            _currentBoid.Velocity.y += _turnFactor;
        }
        if (position.y > -_outerBounds.y)
        {
            _currentBoid.Velocity.y -= _turnFactor;
        }
    }
}
