using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rewardContde : MonoBehaviour
{
	[HideInInspector]
	public float a=1f;
	public float b=-3.2f;
	public float c=2.7f;
	public float k=0.01f; 
	[Tooltip("Time second of seeking time")] public float T=50;

	public float DecayCompetitiveR(in float x, in string team) // x for time 
	{
		if (team == "upend") {
			a = 0f;
		}
		if (team == "upbegin") {
			a = 1f;
		}
		var C1 = Mathf.Exp((-b * x + T * c) / T );
		var C2 = Mathf.Exp((b * x + c) / T);
		var Reward = k + C1 * (1 - a) + C2 * a;
		return Reward;
	}
	public float QuadCompetitiveR(in float x, in string team)
	{
		if (team == "upend") {
			a = 0f;
		}
		if (team == "upbegin") {
			a = -1f;
		}
		var Reward = ((1 / T) * x + a) * ((1 / T) * x + a);
		return Reward;
	}
}
