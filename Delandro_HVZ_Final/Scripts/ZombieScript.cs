using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieScript : Vehicle
{	
	public override void CalcSteeringForces()
	{
		Vector3 finalVector = new Vector3();


		if (inBounds)
		{
			//avoid obstacles
			foreach (Obstacle obstacle in AgentManager.instance.obstacles)
			{
				finalVector += AvoidObstacle(obstacle);
			}
			for (int i = 0; i < AgentManager.instance.zombies.Count; i++)
			{
				finalVector += Separate(AgentManager.instance.zombies[i].position, 1.5f);
			}
			//if target exists, seek the target
			if (target != null)
			{
				finalVector += Pursue(target);
			}
			else {
				finalVector += Wander();
			}
			
			
		}
		else {
			//out of bounds, seek the center
			finalVector += Seek(Vector3.zero);
		}
		finalVector *= base.maxSpeed;
		ApplyForce(finalVector);
	}

}
