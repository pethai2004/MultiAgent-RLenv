using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

using System.Collections.Generic;

public enum Team
{
    Blue = 0,
    Purple = 1
}

public class AgentSoccer : Agent
{

    public enum Position{Striker, Goalie}

    [HideInInspector]
    public Team team;
    float m_KickPower;

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
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    public bool useVisualObservation = false;
 
    EnvironmentParameters m_ResetParams;

    //miscell
    private bool onFloor;

    public override void Initialize()
    {
        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();

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
            m_jumpPotential = 1.7f;
        
        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];
        var verticleAxis = act[3];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * m_ForwardSpeed;
                m_KickPower = 1f;
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
                }
                break;
            case 2:
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
            ForceMode.VelocityChange);
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
    }

    void OnCollisionEnter(Collision c)
    {
        var force = k_Power * m_KickPower;
        if (position == Position.Goalie)
        {
            force = k_Power;
        }
        if (c.gameObject.CompareTag("ball"))
        {
            //AddReward(.2f * m_BallTouch);
            AddReward(.2f);
            var dir = c.contacts[0].point - transform.position + new Vector3(0, 0, Random.Range(0, 2f)); //new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(0f, 1f));
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


        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();
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

}
