using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanScript : Vehicle
{
	public float targetDistance = 10000;
	public override void CalcSteeringForces()
	{

		Vector3 finalVector = new Vector3();

		//checks to see if in bounds 
		if (inBounds)
		{
			foreach(Obstacle obstacle in AgentManager.instance.obstacles)
			{
				finalVector += AvoidObstacle(obstacle);
			}
			for (int i = 0; i < AgentManager.instance.humans.Count; i++) {
				finalVector += Separate(AgentManager.instance.humans[i].position, 1.5f);
			}
			//flee from closest zombie if closest zombie is a threat
			if (target != null && targetDistance <= 6)
			{
				finalVector += Evade(target);
			}
			else
			{
				finalVector+=Wander();
			}
		}
		else {
			//not in bounds, seek center
			finalVector += Seek(Vector3.zero);
		}
		
		finalVector *= maxSpeed;
		ApplyForce(finalVector);
	}	
}
