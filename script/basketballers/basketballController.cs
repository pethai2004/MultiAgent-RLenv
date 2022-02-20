using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basketballController : MonoBehaviour
{
	public GameObject area;
	[HideInInspector]
	public EnvBasketController envController;
	public string purpleGoalTag;
	public string blueGoalTag;

	public string purpleGoalTagLast;
	public string blueGoalTagLast;

	public bool triggerOneOn = false;
	void Start()
	{
		envController = area.GetComponent<EnvBasketController>();
	}

	void OnTriggerEnter(Collider col)
	{	
		if (col.gameObject.CompareTag(purpleGoalTag))
		{
			triggerOneOn = true;
			Debug.Log("tricker1-purple");
		}
		//////////////////////////
		if (col.gameObject.CompareTag(blueGoalTag)) 
		{
			triggerOneOn = true;
			Debug.Log("tricker1-blue");
		}
		/////////////////
		if (triggerOneOn)
		{
			if (col.gameObject.CompareTag(purpleGoalTagLast))
			{
				Debug.Log("tricker2-purple");
				triggerOneOn = false;
				envController.GoalTouched(Team.Blue);
			}
			if (col.gameObject.CompareTag(blueGoalTagLast)) 
			{
				Debug.Log("tricker2-blue");
				triggerOneOn = false;
				envController.GoalTouched(Team.Purple);
			}
		}

	}

}
