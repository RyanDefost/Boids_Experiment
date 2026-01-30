using System.Runtime.InteropServices;
using UnityEngine;


public class CrowdSimulationController : MonoBehaviour
{
    #region
    [Header("Simulation")]
    [SerializeField] int agentCount = 10000;
    [SerializeField] float worldSize = 50f;

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
    }

    private void initializeAgents()
    {
        agentBuffer = new ComputeBuffer(agentCount, Marshal.SizeOf<CrowdAgent>());

        var agents = new CrowdAgent[agentCount];
        for (int i = 0; i < agentCount; i++)
        {
            agents[i] = new CrowdAgent
            {
                position = Random.insideUnitCircle * worldSize,
                velocity = Vector2.zero,
                target = Random.insideUnitCircle * worldSize,
                maxSpeed = Random.Range(2f, 4f)
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
    public float maxSpeed;
}