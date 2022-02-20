using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class functionalGameController : MonoBehaviour
{
	public GameObject thisthing;
	public string typeofWhat; // speedo, hater

	void ThrowSomething(in Vector3 whereTo)
	{
		// thisthing.GetComponent<Rigidbody>().AddForce(whereTo * 10f, 10f, ForceMode.VelocityChange);
	}
}
