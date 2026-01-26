using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private int agentAmount = 0;
    [SerializeField] private Vector2 agentArea = new Vector2(20, 20);

    [SerializeField] private GameObject agentObject;

    //private List<Agent> agents = new List<Agent>();
    private Dictionary<Agent, Vector2> agents = new Dictionary<Agent, Vector2>();
    //
    public int gridSize { get; private set; }

    private void Start()
    {
        gridSize = 6;
        CreateAgent(this.agentAmount);
    }

    // Update is called once per frame
    private void Update()
    {
        for (int i = 0; i < agents.Keys.Count; i++)
        {
            var currentAgent = agents.ElementAt(i).Key;

            currentAgent.UpdateAgent();
            agents[currentAgent] = currentAgent.GridPosition;
        }
    }

    public Dictionary<Agent, Vector2> GetAgents() => this.agents;

    private void CreateAgent(int amount)
    {
        if (amount <= 0) return;

        for (int i = 0; i < amount; i++)
        {
            var randomPosition = new Vector3(
                Random.Range(-agentArea.x, agentArea.x),
                Random.Range(-agentArea.y, agentArea.y),
                0
            );

            var currentObject = Instantiate(agentObject, randomPosition, transform.rotation);
            var currentAgent = currentObject.GetComponent<Agent>();

            var gridPositionX = Mathf.Floor(currentAgent.transform.position.x / gridSize);
            var gridPositionY = Mathf.Floor(currentAgent.transform.position.y / gridSize);

            this.agents.Add(currentAgent, new Vector2(gridPositionX, gridPositionY));
        }
    }
}
