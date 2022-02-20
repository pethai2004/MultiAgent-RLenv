using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
// blue is hider and purple is seeker
public class EnvSandHController : MonoBehaviour
{
	public List<GameObject> obsCubePrefabs = new List<GameObject>();
	//public List<GameObject> sparwnObjections = new List<GameObject>();

	public float TimeToHide=12f;
	public float TimeToSeek=100f;

    public Vector3 BinFoundAgent = new Vector3(22, 10, 22);
    public Vector3 startSeekerPos = new Vector3(0, 10, 10);
    [HideInInspector]

	[System.Serializable]
    public class PlayerInfoHider 
    {
        public AgentHider Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }
    [System.Serializable]
    public class PlayerInfoSeeker
    {
        public AgentSeeker Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    public List<PlayerInfoHider> AgentsHiderList = new List<PlayerInfoHider>();
    public List<PlayerInfoSeeker> AgentSeekerList = new List<PlayerInfoSeeker>();

    private SimpleMultiAgentGroup m_SeekerAgentGroup;
    private SimpleMultiAgentGroup m_HiderAgentGroup;

    public Transform spawnobsparent;

    [HideInInspector]
    public bool InStateOfHide;
    [HideInInspector] public float TimeLeftToHide;
    [HideInInspector] public float TimeLeftToSeek;
    [HideInInspector] public int agentLeft;
    public int splengthObs=5;

    void Start()
    {
    	m_SeekerAgentGroup = new SimpleMultiAgentGroup();
    	m_HiderAgentGroup = new SimpleMultiAgentGroup();

    	foreach (var item in AgentsHiderList) {
    		item.StartingPos = item.Agent.transform.position;
    		item.StartingRot = item.Agent.transform.rotation;
    		item.Rb = item.Agent.GetComponent<Rigidbody>();
            m_HiderAgentGroup.RegisterAgent(item.Agent);
    	}
        foreach (var item in AgentSeekerList) {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            m_SeekerAgentGroup.RegisterAgent(item.Agent);
        }
    	ResetScene();
    }
    void ResetScene()
    {
        InStateOfHide = true;
        TimeLeftToSeek = TimeToSeek;
        TimeLeftToHide = TimeToHide;
        agentLeft = AgentsHiderList.Count;

        foreach (var item in AgentsHiderList) {
            item.Agent.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            item.Agent.transform.position = BinFoundAgent; //new Vector3(Random.Range(-20f, 20f), 1f, Random.Range(-20f, 20f));;
            item.Agent.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 180f), 0);
        }
        foreach (var item in AgentSeekerList) { // freeze seeker left hider to hide
            item.Agent.transform.position = startSeekerPos;
            item.Agent.transform.rotation = Quaternion.identity;
            item.Agent.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
        spawnObsCube();
    }
    public void FoundOneHider(in GameObject hider) // call by agent script
    {
    	agentLeft -= 1;
        hider.gameObject.transform.position = BinFoundAgent;
        hider.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    	//hear implement what heppen when discovered the hider
        if (agentLeft == 0) {
            m_HiderAgentGroup.AddGroupReward(-2f);
            m_SeekerAgentGroup.AddGroupReward(2f);
            m_HiderAgentGroup.EndGroupEpisode();
            m_SeekerAgentGroup.EndGroupEpisode();
            ResetScene();
        }
    }
    void Update()
    {
        ControlTimer();
    }

    public void ControlTimer()
    {   
        if (InStateOfHide) {
            TimeLeftToHide -= Time.deltaTime ;
            
            if (TimeLeftToHide <= 0) {
                InStateOfHide = false;
                foreach (var item in AgentSeekerList) {
                    item.Agent.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    item.Agent.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                }
            }
        }
        if (InStateOfHide == false) {
            TimeLeftToSeek = TimeLeftToSeek - Time.deltaTime ;
            
            if (TimeLeftToSeek<=0 && agentLeft >= 0) {
                m_HiderAgentGroup.AddGroupReward(3f);
                m_SeekerAgentGroup.AddGroupReward(-3f);
                m_HiderAgentGroup.EndGroupEpisode();
                m_SeekerAgentGroup.EndGroupEpisode();
                Debug.Log("seekerLost");
                ResetScene();
            }
        }
    }
    public float GetCurrentSHtimeinterval()
    {
        float min = Mathf.FloorToInt(TimeLeftToSeek / 60) * 60;
        float sec = Mathf.FloorToInt(TimeLeftToSeek % 60);
        return min + sec;
    }
    void CreateMapRandomEasy()
    {

    }
    public void spawnObsCube()
    {   
        if (spawnobsparent.transform.childCount >= 1){
                for (int i = 0; i<= splengthObs; i++) {
                     GameObject.Destroy(spawnobsparent.transform.GetChild(i).gameObject);
                }
            }
        var random = new System.Random();
        for (int x = 0; x<=splengthObs; x++) {
            GameObject Copysp;
            var i_r = random.Next(obsCubePrefabs.Count);
            var targetingObs = obsCubePrefabs[i_r]; //targetingObs.transform.localScale
            Vector3 SPposition = new Vector3(Random.Range(-20, 20), 2, Random.Range(-20, 20));
            Copysp = Instantiate<GameObject>(targetingObs, SPposition, Quaternion.identity, spawnobsparent);
        }
    }  

    // private Vector3 GetPositionRandom(){
    //     Collider[] overlaped = Physics.OverlapBox(gameObject.transform.position, transform.localScale / 2);
    //     int i = 0;
    //     while( i < overlaped.Length){
    //         Debug.Log("overlapedNow");
    //         i++;
    //     }
    //     return Vector3.zero;
    // }
}
