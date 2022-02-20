using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;



public class AgentBasketballer : Agent
{
	public enum Position{Striker, Goalie}
    [HideInInspector]
    public Team team;

    public Position position;

    const float k_Power = 4000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;
    float m_jumpPotential;

    public GameObject ball;
    public GameObject opponentGoal;
    public GameObject ourGoal;

    [HideInInspector]
    public Rigidbody agentRb;
    bsksetting m_baskSeting;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    public bool useVisualObservation = false;
 
    EnvironmentParameters m_ResetParams;

    //miscell
    private bool onFloor;

    public override void Initialize()
    {
        EnvBasketController envController = GetComponentInParent<EnvBasketController>();

        if (envController != null)
        {
            m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            m_Existential = 1f / MaxStep;
        }

        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();

        if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.position.x - 5f, .5f, transform.position.z);
            rotSign = 1f;
        }
        else
        {
            team = Team.Purple;
            initialPos = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotSign = -1f;
        }
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.3f;
            m_jumpPotential = 3f;
        
        m_baskSeting = FindObjectOfType<bsksetting>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 400;

        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];
        var verticleAxis = act[3];
        var shootorNot = act[4];

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
                //dirToGo = transform.up * m_jumpPotential;
                if (onFloor==true)
                {
                    dirToGo =  transform.up * m_jumpPotential;
                    //onFloor = false;
                    Debug.Log("jumped");
                }
                break;
            case 2:
                break;
        }

        switch (shootorNot)
        {
        	case 1:
        		shootBall(new Vector3(dirToGo.x * 2, 3, dirToGo.y * 2));
        		break;

        	case 2:
        		break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        agentRb.AddForce(dirToGo * m_baskSeting.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    public void shootBall(in Vector3 directionshoot)
    {
    	//Debug.Log(Vector3.Distance(transform.position, ball.transform.position));

    	if (Vector3.Distance(transform.position, ball.transform.position) < 3)
    	{
    		var towhere = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 
    			Random.Range(0f, 0.1f));
    	    ball.GetComponent<Rigidbody>().AddForce(directionshoot * 400 + towhere);
    	}
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (position == Position.Goalie)
        {
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            AddReward(-m_Existential);
        }
        MoveAgent(actionBuffers.DiscreteActions);
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
        discreteActionsOut[4] = 2;
        if (Input.GetKey(KeyCode.P))
        {
        	discreteActionsOut[4] = 1;
        }
    }

    void OnCollisionEnter(Collision c)
    {
        var force = 100;

        if (c.gameObject.CompareTag("ball"))
        {
            //AddReward(.2f * m_BallTouch);
            AddReward(.2f);
            var dir = c.contacts[0].point - transform.position;
            // normalized and at some noise to kicking direction.
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }

        // for limit agent jump
        if (c.gameObject.CompareTag("ground"))
        {
            onFloor = true;
        }
    }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("ground"))
        {
            onFloor = false;
        }
    }
    public override void OnEpisodeBegin()
    {
        //m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        var meToBall = Vector3.Distance(ball.transform.position, transform.position); // distance from this agent to ball 
        var ballToOppoGoal = Vector3.Distance(ball.transform.position, opponentGoal.transform.position); // distance from ball to opponent goal
        var ballToOurGoal = Vector3.Distance(ball.transform.position, ourGoal.transform.position); // distance from ball to our goal
        var ballVelocity = ball.GetComponent<Rigidbody>().velocity; // ball velocity

        sensor.AddObservation(meToBall); //1
        sensor.AddObservation(ballToOppoGoal);//1
        sensor.AddObservation(ballToOurGoal);//1
        sensor.AddObservation(ballVelocity); //3


        if (meToBall <= 7.0f ){ AddReward(1 / meToBall); }
        if (ballToOppoGoal <= 3.0f ){ AddReward( 1 / ballToOppoGoal); }
        if (ballToOurGoal <= 3.0f ){ AddReward(- 1 / ballToOurGoal); }


        EnvBasketController envController = GetComponentInParent<EnvBasketController>();
        //var allAgentRelativeOrigin = new List<float>();

        foreach (var ag in envController.AgentsList)
        {
            var positionAg = ag.Agent.transform.position;

            //if (ag.Agent.team != team)//######### hear we need to make sure that number of purple equal to blue team, if not then observation shape would be different - blue and purple
            // so check twice!! if use each agent position as obs.

            //add all agent regardless of the team
            var distTous = Vector3.Distance(positionAg, transform.position);
            var distToball = Vector3.Distance(positionAg, ball.transform.position);
            var distTogoal = Vector3.Distance(positionAg, ourGoal.transform.position);
        
            sensor.AddObservation(positionAg);//3
            sensor.AddObservation(distTous);//1
            sensor.AddObservation(distToball);//1
            sensor.AddObservation(distTogoal);//1
        }
    }

    public void bounceBall()
    {

    }
}
