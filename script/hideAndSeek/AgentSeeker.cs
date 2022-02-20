using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public class AgentSeeker : Agent
{	
	public float m_agentRunSpeed=.5f;
	[HideInInspector]
	public Rigidbody agentRb;
	BehaviorParameters m_BehaviorParameters;
	public Vector3 initialPos;
	private rewardContde rewfunc;

	public EnvSandHController envController;
	public rayController rayCastController;
	// miscell
	public bool onFloor=false;
	public string team = "Seeker";

	[HideInInspector] 
    float m_LateralSpeed;
    float m_ForwardSpeed;
    float m_jumpPotential;

    public override void Initialize()
    {
    	envController = GetComponentInParent<EnvSandHController>();
    	rayCastController = GetComponentInChildren<rayController>();
    	m_BehaviorParameters = GetComponent<BehaviorParameters>();
    	rewfunc = GetComponent<rewardContde>();

		initialPos = new Vector3(transform.position.x, 1f, transform.position.z);
        m_LateralSpeed = .7f;
        m_ForwardSpeed = .7f;
        m_jumpPotential = 1.2f;
    	agentRb = GetComponent<Rigidbody>();
    	agentRb.maxAngularVelocity = 300;	
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
    	if (envController.InStateOfHide == false) { //decay reward for only seeking period
    		var r = rewfunc.DecayCompetitiveR(envController.GetCurrentSHtimeinterval(), "upbegin");
            AddReward(r);
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
                if (onFloor==true){
                    dirToGo =  transform.up * m_jumpPotential;
                }
                break;
            case 2:
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        agentRb.AddForce(dirToGo * m_agentRunSpeed, ForceMode.VelocityChange);
    }
        
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W)){
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S)){
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A)){
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D)){
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E)){
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q)){
            discreteActionsOut[1] = 2;
        }
        discreteActionsOut[3] = 2;
        if (Input.GetKey(KeyCode.J)){
            discreteActionsOut[3] = 1;
        }
    }
    public void foundYouOrNotHider()
    {
    	rayCastController.SimpleRay();
    	if (rayCastController.currentHitObj != null && rayCastController.currentHitObj.CompareTag("HiderAgent")) {
            var r = rewfunc.QuadCompetitiveR(envController.GetCurrentSHtimeinterval(), "upbegin") + 0.5f;
    		AddReward(r);
    		var foundWho = rayCastController.currentHitObj;
    		rayCastController.currentHitObj.GetComponent<AgentHider>().beenFound();
    		envController.FoundOneHider(foundWho);
    	}
    }
    public void FixedUpdate(){
        if (envController.InStateOfHide == false){
            foundYouOrNotHider();
        }
    }
    void OnCollisionEnter(Collision c) {
    if (c.gameObject.CompareTag("ground") || (c.gameObject.CompareTag("obstracle"))) {
            onFloor = true;
        }
    }

    void OnCollisionExit(Collision c){
        if (c.gameObject.CompareTag("ground") || (c.gameObject.CompareTag("obstracle")))
        {
            onFloor = false;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var item in envController.AgentsHiderList) {
            var positionAgent = item.Agent.transform.position;
            var distanceToAgent = Vector3.Distance(item.Agent.transform.position, transform.position);
            var rotationAgent = item.Agent.transform.rotation;
            sensor.AddObservation(rotationAgent);
            sensor.AddObservation(positionAgent);
            sensor.AddObservation(distanceToAgent);
        }
        foreach (var item in envController.AgentSeekerList) {
            var positionAgent = item.Agent.transform.position;
            var distanceToAgent = Vector3.Distance(item.Agent.transform.position, transform.position);
            var rotationAgent = item.Agent.transform.rotation;
            sensor.AddObservation(rotationAgent);
            sensor.AddObservation(positionAgent);
            sensor.AddObservation(distanceToAgent);
        }
        for (int i = 0; i<= envController.splengthObs; i++) {
            var objects = envController.spawnobsparent.transform.GetChild(i).gameObject;
            sensor.AddObservation(objects.transform.position);
            sensor.AddObservation(objects.transform.rotation);
        }
    }
}
