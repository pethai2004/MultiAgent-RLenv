using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
// blue 0, purple 1
public class AgentTandR : Agent
{
	public float m_agentRunSpeed=1;
	[HideInInspector] // Let team blue be Jerry and purple be Tom
	public Team team;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;
    float m_jumpPotential;

    public EnvTandRController envController;

    [HideInInspector]
    public Rigidbody agentRb;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;

    private bool onFloor;
    // public GameObject[] pickUpWhat;

    public override void Initialize()
    {
    	envController = GetComponentInParent<EnvTandRController>();
    	m_BehaviorParameters = GetComponent<BehaviorParameters>();
    	m_Existential = 1f / envController.MaxEnvironmentSteps;

    	if (m_BehaviorParameters.TeamId == (int)Team.Blue)
    	{
    		team = Team.Blue;
    		initialPos = new Vector3(transform.position.x + 10, 1f, transform.position.z);
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.3f;
            m_jumpPotential = 1.5f;
    	}
    	else
    	{
    		team = Team.Purple;
    		initialPos = new Vector3(transform.position.x - 10, 1f, transform.position.z);
    		m_LateralSpeed = 1.1f;
            m_ForwardSpeed = 1.5f;
            m_jumpPotential = 2.5f; // let give some advantage here haha
    	}
    	agentRb = GetComponent<Rigidbody>();
    	agentRb.maxAngularVelocity = 250;	
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {	
    	if (team == Team.Blue) // for Jerry
        {
            AddReward(m_Existential);
        }
        else if (team == Team.Purple) // for Tom
        {
            AddReward(-m_Existential);
        }
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public void MoveAgent(ActionSegment<int> act)
    {    
    	var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];
        var verticleAxis = act[3];
        // var pickOrNot = act[4];
        

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * m_ForwardSpeed;
                break;
            case 2:
                dirToGo = transform.forward * -m_ForwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * m_LateralSpeed;
                break;
            case 2:
                dirToGo = transform.right * -m_LateralSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

        switch (verticleAxis)
        {
            case 1:
                if (onFloor==true)
                {
                    dirToGo =  transform.up * m_jumpPotential;
                }
                break;
            case 2:
                break;
        }
 
        // switch (pickOrNot)
        // {
        // 	case 1:
        // 		// PickTool();
        // 		break;
        // 	case 2:
        // 		break;
        // }
        
        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        agentRb.AddForce(dirToGo * m_agentRunSpeed, ForceMode.VelocityChange);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }
        discreteActionsOut[3] = 2;
        if (Input.GetKey(KeyCode.J))
        {
            discreteActionsOut[3] = 1;
        }
        // discreteActionsOut[4] = 2; //this is provided for pickup tool

        // if (Input.GetKey(KeyCode.N))
        // {
        // 	discreteActionsOut[4] = 1;
        // }
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("blueAgent"))
        {
            AddReward(1f);
            envController.HitJerry(c);
            c.gameObject.GetComponent<AgentTandR>().AddReward(-1f);
        }
        // for limit agent jump
        if (c.gameObject.CompareTag("ground"))
        {
            onFloor = true;
        }
    }
    // private void OnTriggerEnter(Collider c)
    // {
    //     if (c.gameObject.CompareTag("rewHandS")){
    //         PickTool(c);
    //     }
    // }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("ground"))
        {
            onFloor = false;
        }
    }
    // void PickTool(in Collision c)
    // {
    //     if (c.gameObject.GetComponent<functionalGameController>().typeofWhat == "speedo") // if jerry then add speed
    //     {
    //         m_agentRunSpeed = 1.3;
    //         m_jumpPotential = 2.7;
    //     }
    //     if (c.gameObject.GetComponent<functionalGameController>().typeofWhat == "hater")
    //     {
    //         c.Transform.parent = gameObject;
    //         c.position = new Vector3(-1, 0, 0);
    //         haveBall=true
    //     }
    // }
    // void ResetTool()
    // {
    //     m_agentRunSpeed = 1.0f;
    //     m_jumpPotential = 2.4f;
    //     haveBall=false;
    //     timerOfFunc = 0;
    // }
    public override void CollectObservations(VectorSensor sensor)
    {
    	foreach (var item in envController.AgentsList)
    	{
    		var positionAgent = item.Agent.transform.position;
    		var distanceToAgent = Vector3.Distance(item.Agent.transform.position, transform.position);
    		var alertness = 0.01f;
    		if (item.Agent.team != team)
    		{
    			alertness = Mathf.Exp( - distanceToAgent);
    		}
    		var velcAgent = item.Agent.agentRb.velocity;
    		var rotAgent = item.Agent.agentRb.angularVelocity;

    		sensor.AddObservation(positionAgent);
    		sensor.AddObservation(distanceToAgent);
    		sensor.AddObservation(alertness);
    		sensor.AddObservation(velcAgent);
    		sensor.AddObservation(rotAgent);
    	}
    }
}
