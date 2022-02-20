using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class EnvTandRController : MonoBehaviour
{
	public List<GameObject> obsCubePrefabs = new List<GameObject>();
	// public List<GameObject> sparwnObjections = new List<GameObject>();
    public Transform spawnobsparent;

	[System.Serializable]
    public class PlayerInfo
    {
        public AgentTandR Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 3000;
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;
    private int m_ResetTimer;

    private int jerryDeath = 0;

   	public int sparwnObjectInterval = 100;
    public int splengthObs=3;

    void Start()
    {
    	m_BlueAgentGroup = new SimpleMultiAgentGroup();
    	m_PurpleAgentGroup = new SimpleMultiAgentGroup();

    	foreach (var item in AgentsList)
    	{
    		item.StartingPos = item.Agent.transform.position;
    		item.StartingRot = item.Agent.transform.rotation;
    		item.Rb = item.Agent.GetComponent<Rigidbody>();
            if (item.Agent.team == Team.Blue)
            {
                m_BlueAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                m_PurpleAgentGroup.RegisterAgent(item.Agent);
            }
    	}
    	ResetScene();
    }
    public void ResetScene()
    {
    	m_ResetTimer = 0;
    	foreach (var item in AgentsList)
    	{
    		//set new position and rotation of agent
            var newStartPos = item.Agent.initialPos + new Vector3(Random.Range(-23f, 23f), 1f, Random.Range(-23f, 23f));
            var newRot = Quaternion.Euler(0, Random.Range(0.0f, 100.0f), 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
    	}
    	spawnObsCube(obsCubePrefabs);
        // spawnObsCube(sparwnObjections);
    }

    public void HitJerry(Collision coljerry)
    {
    	jerryDeath += 1;
    	var dir = coljerry.contacts[0].point;
            // here actually does not neccesary just for fun (kick jerry and destroy object)
        coljerry.gameObject.GetComponent<Rigidbody>().AddForce(dir.normalized * 20);
        coljerry.gameObject.transform.position = new Vector3(Random.Range(-23f, 23f), 1f, Random.Range(-23f, 23f));

    	m_PurpleAgentGroup.AddGroupReward(1f - (float)m_ResetTimer / MaxEnvironmentSteps); // positive reward for Tom
    	m_BlueAgentGroup.AddGroupReward(-1f + (float)m_ResetTimer / MaxEnvironmentSteps); // negative reward for Jerry
    	// end episode since Jerry is died all.
    }
    public void spawnObsCube(in List<GameObject> SPwhat)
    {   
        if (spawnobsparent.transform.childCount >= 1){
                for (int i = 0; i<= splengthObs; i++) {
                     GameObject.Destroy(spawnobsparent.transform.GetChild(i).gameObject);
                }
            }
        var random = new System.Random();
        for (int x = 0; x<=splengthObs; x++) {
            GameObject Copysp;
            var i_r = random.Next(SPwhat.Count);
            var targetingObs = SPwhat[i_r]; //targetingObs.transform.localScale
            Vector3 SPposition = new Vector3(Random.Range(-20, 20), 2, Random.Range(-20, 20));
            Copysp = Instantiate<GameObject>(targetingObs, SPposition, Quaternion.identity, spawnobsparent);
        }
    }

	void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
    		m_BlueAgentGroup.AddGroupReward((float) -jerryDeath);
    		m_PurpleAgentGroup.AddGroupReward((float) jerryDeath);
    		m_PurpleAgentGroup.EndGroupEpisode();
    		m_BlueAgentGroup.EndGroupEpisode();
            ResetScene();
        }

    }
}

