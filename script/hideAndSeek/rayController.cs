using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rayController : MonoBehaviour
{
	[HideInInspector]
	public GameObject currentHitObj=null;
	public float currHitDist;

	public float sphereRadius;
	public float maxDist;

	private Vector3 originRay;
	private Vector3 directionRay;

	public LayerMask layerMask;

	[HideInInspector]
	public int RaysToShoot = 10;
	public void SimpleRay()
	{  
		originRay = transform.position; //- new Vector3(0, 0, 1) *  2f;
		directionRay = transform.forward;

		RaycastHit hit;

		if (Physics.SphereCast(originRay, sphereRadius, directionRay, out hit, maxDist, layerMask, 
			QueryTriggerInteraction.UseGlobal))
		{
			currentHitObj = hit.transform.gameObject;
			currHitDist = hit.distance;
		}
		else {
			currHitDist = maxDist;
			currentHitObj = null;
		}
	}
	public void MultiRay()
	{
		// TODO: implement arbitrary degree of raycast
	}
    private void OnDrawGizmosSelected()
    {
    	Gizmos.color=Color.red;
    	Debug.DrawLine(originRay, originRay + directionRay * currHitDist);
    	Gizmos.DrawWireSphere(originRay + directionRay * currHitDist, sphereRadius);
    }
}
