using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

public class CrowdSimulationController : MonoBehaviour
{
    #region
    [Header("Simulation")]
    [SerializeField] int agentCount = 10000;
    [SerializeField] float worldSize = 50f;

    [Space]
    [SerializeField] float _cohesionFactor = 0.0005f;
    [SerializeField] float _alignmentFactor = 0.1f;
    [SerializeField] float _avoidanceFactor = 0.1f;
    [SerializeField] Vector2 _outerBounds = new Vector2(100, 100);

    [Header("Rendering")]
    [SerializeField] Mesh agentMesh;
    [SerializeField] Material agentMaterial;

    [Header("Compute")]
    [SerializeField] ComputeShader crowdShader;

    ComputeBuffer agentBuffer;
    int moveKernel;
    Bounds drawBounds;

    const string MoveKernelName = "MoveAgents";
    static readonly int AgentCountID = Shader.PropertyToID("agentCount");
    static readonly int DeltaTimeID = Shader.PropertyToID("deltaTime");
    static readonly int AgentsID = Shader.PropertyToID("agents");

    static readonly int CohesionFactorID = Shader.PropertyToID("_cohesionFactor");
    static readonly int AlignmentFactorID = Shader.PropertyToID("_alignmentFactor");
    static readonly int AvoidanceFactorID = Shader.PropertyToID("_avoidanceFactor");
    static readonly int OuterBoundsID = Shader.PropertyToID("_outerBounds");

    #endregion

    private void Start()
    {
        initializeAgents();
        InitializeCompute();
        InitializeRender();
    }

    private void Update()
    {
        crowdShader.SetFloat(DeltaTimeID, Time.deltaTime);
        DispatchSimulation();
        DrawAgents();
    }

    private void OnDestroy() => agentBuffer?.Release();

    private void DrawAgents()
    {
        Graphics.DrawMeshInstancedProcedural(
            agentMesh,
            0,
            agentMaterial,
            drawBounds,
            agentCount
        );
    }

    private void DispatchSimulation()
    {
        var threadGroups = Mathf.CeilToInt(agentCount / 256f);
        crowdShader.Dispatch(moveKernel, threadGroups, 1, 1);
    }

    private void InitializeRender()
    {
        agentMaterial.SetBuffer(AgentsID, agentBuffer);
        drawBounds = new Bounds(Vector3.zero, Vector3.one * worldSize * 2f);
    }

    private void InitializeCompute()
    {
        moveKernel = crowdShader.FindKernel(MoveKernelName);

        crowdShader.SetInt(AgentCountID, agentCount);
        crowdShader.SetBuffer(moveKernel, AgentsID, agentBuffer);

        //Settings
        crowdShader.SetFloat(CohesionFactorID, _cohesionFactor);
        crowdShader.SetFloat(AlignmentFactorID, _alignmentFactor);
        crowdShader.SetFloat(AvoidanceFactorID, _avoidanceFactor);
        crowdShader.SetVector(OuterBoundsID, _outerBounds);
    }

    private void initializeAgents()
    {
        agentBuffer = new ComputeBuffer(agentCount, Marshal.SizeOf<CrowdAgent>());

        var agents = new CrowdAgent[agentCount];
        for (int i = 0; i < agentCount; i++)
        {
            var newPosition = UnityEngine.Random.insideUnitCircle * worldSize;

            Vector2 randomDiraction = new Vector2(
                UnityEngine.Random.Range(-1, 2),
                UnityEngine.Random.Range(-1, 2)
            );
            randomDiraction = randomDiraction == Vector2.zero ? Vector2.one : randomDiraction;

            var gridPosition = new Vector2(
                Mathf.Floor(newPosition.x / 6),
                Mathf.Floor(newPosition.y / 6)
            );

            var currentColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

            agents[i] = new CrowdAgent
            {
                position = newPosition,
                velocity = randomDiraction,
                target = UnityEngine.Random.insideUnitCircle * worldSize,
                gridPosition = gridPosition,
                teamColor = new float4(currentColor.r, currentColor.g, currentColor.b, currentColor.a),
                maxSpeed = UnityEngine.Random.Range(2f, 4f)
            };
        }

        agentBuffer.SetData(agents);
    }
}

[StructLayout(LayoutKind.Sequential)]
partial struct CrowdAgent
{
    public Vector2 position;
    public Vector2 velocity;
    public Vector2 target;
    public Vector2 gridPosition;
    public float4 teamColor;
    public float maxSpeed;
}