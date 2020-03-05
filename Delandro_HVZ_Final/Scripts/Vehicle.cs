using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Vehicle : MonoBehaviour
{
	// Vectors for the physics
	public Vector3 position;
	public Vector3 direction;
	public Vector3 velocity;
	public Vector3 acceleration;

	public GameObject zombieFuturePositionPrefab;
	public GameObject humanFuturePositionPrefab;
	private GameObject zombieFuturePosition;
	private GameObject humanFuturePosition;

	public float wanderTime = 2.5f;

	private Vector3 wanderCircleLocation;
	private Vector3 wanderCircleSpot;
	private Vector3 wanderLocation;
	public float wanderCircleRadius = 9f;
	public int wanderCircleAngle;
	public int wanderCircleAngleOffset = 90;

	public Material forwardLine;
	public Material rightLine;
	public Material chasingLine;


	public Vehicle target = null;

	// The mass of the object. Note that this can't be zero
	public float mass = 1;

	public float maxSpeed = 4;

	private const float MIN_VELOCITY = 0.1f;

	protected bool inBounds = true;

	public float safeDistance = 3f;

	public bool isSeeking = true;

	protected void Start()
	{
		// Initialize all the vectors
		position = transform.position;
		direction = Vector3.right;
		velocity = Vector3.zero;
		acceleration = Vector3.zero;

		zombieFuturePosition = Instantiate(zombieFuturePositionPrefab);
		humanFuturePosition = Instantiate(humanFuturePositionPrefab);
	}

	protected void Update()
	{

		CalcSteeringForces();
		// Then, calculate the physics
		UpdatePhysics();
		// Make sure the vehicle stays on screen (remove this for the exercise)
		CheckBoundaries();
		//Wrap();
		// Finally, update the position
		UpdatePosition();

		DrawFuturePositions();
	}

	private void DrawFuturePositions() {
		if (AgentManager.instance.linesOn)
		{

			if (tag == "Zombie")
			{
				if(!zombieFuturePosition.activeSelf)zombieFuturePosition.SetActive(true);
				zombieFuturePosition.transform.position = GetFuturePosition(1.4f);
			}
			else
			{
				if(!humanFuturePosition.activeSelf)humanFuturePosition.SetActive(true);
				humanFuturePosition.transform.position = GetFuturePosition(1.4f);
			}
		}
		else {
			humanFuturePosition.SetActive(false);
			zombieFuturePosition.SetActive(false);
		}
		
	}

	/// <summary>
	/// Updates the physics properties of the vehicle
	/// </summary>
	protected void UpdatePhysics()
	{
		// Add acceleration to velocity, and have that be scaled with time
		velocity += acceleration * Time.deltaTime;

		// Change the position based on velocity over time
		position += velocity * Time.deltaTime;

		// Calculate the direction vector
		direction = velocity.normalized;



		// Reset the acceleration for the next frame
		acceleration = Vector3.zero;
	}

	/// <summary>
	/// Wraps the vehicle around the screen
	/// </summary>
	protected void Bounce()
	{
		Camera cam = Camera.main;
		Vector3 max = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, cam.nearClipPlane));
		Vector3 min = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));

		if (position.x > max.x && velocity.x > 0)
		{
			velocity.x *= -1;
		}
		if (position.y > max.y && velocity.y > 0)
		{
			velocity.y *= -1;
		}
		if (position.x < min.x && velocity.x < 0)
		{
			velocity.x *= -1;
		}
		if (position.y < min.y && velocity.y < 0)
		{
			velocity.y *= -1;
		}
	}

	/// <summary>
	/// Wraps the vehicle around the screen
	/// </summary>
	protected void Wrap()
	{
		Camera cam = Camera.main;
		Vector3 max = new Vector3(18, 1, 18);
		Vector3 min = new Vector3(-18, 1, -18);

		if (position.x > max.x && velocity.x > 0)
		{
			position.x = min.x;
		}
		if (position.z > max.z && velocity.z > 0)
		{
			position.z = min.z;
		}
		if (position.x < min.x && velocity.x < 0)
		{
			position.x = max.x;
		}
		if (position.z < min.z && velocity.z < 0)
		{
			position.z = max.z;
		}
	}

	/// <summary>
	/// Update the vehicle's position
	/// </summary>
	protected void UpdatePosition()
	{
		// Atan2 determines angle of velocity against the right vector
		float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, 0, angle);

		position.y = 1;
		transform.rotation = new Quaternion(0, 0, 0,0);

		// Update position
		gameObject.transform.position = position;
	}

	/// <summary>
	/// Applies friction to the vehicle
	/// </summary>
	/// <param name="coeff">The coefficient of friction</param>
	protected void ApplyFriction(float coeff)
	{
		// If the velocity is below a minimum value, just stop the vehicle
		if (velocity.magnitude < MIN_VELOCITY)
		{
			velocity = Vector3.zero;
			return;
		}

		Vector3 friction = velocity * -1;
		friction.Normalize();
		friction = friction * coeff;
		acceleration += friction;
	}

	/// <summary>
	/// Applies a force to the vehicle
	/// </summary>
	/// <param name="force">The force to be applied</param>
	public void ApplyForce(Vector3 force)
	{
		// Make sure the mass isn't zero, otherwise we'll have a divide by zero error
		if (mass == 0)
		{
			Debug.LogError("Mass cannot be zero!");
			return;
		}

		// Add our force to the acceleration for this frame
		acceleration += force / mass;
	}

	protected Vector3 Seek(Vector3 targetPosition)
	{
		Vector3 desiredVelocity = targetPosition - position;
		desiredVelocity.Normalize();
		desiredVelocity *= maxSpeed;
		Vector3 steeringForce = desiredVelocity - velocity;
		return steeringForce;
	}

	protected Vector3 Seek(Vehicle targetObj)
	{
		return Seek(targetObj.transform.position);
	}

	protected Vector3 Pursue(Vehicle target) {
		Vector3 steeringForce = Seek(target.GetFuturePosition(2f));
		return steeringForce;
	}


	protected Vector3 Flee(Vector3 targetPosition)
	{
		Vector3 desiredVelocity = position - targetPosition;
		desiredVelocity.Normalize();
		desiredVelocity *= maxSpeed;
		Vector3 steeringForce = desiredVelocity - velocity;
		return steeringForce;
	}

	protected Vector3 Flee(GameObject targetObject)
	{
		return Flee(targetObject.transform.position);
	}

	protected Vector3 Evade(Vehicle target) {
		Vector3 steeringForce = Flee(target.GetFuturePosition(1));
		return steeringForce;
	}
	/// <summary>
	/// makes the vehicle stop
	/// </summary>
	public void Stop() {
		velocity = Vector3.zero;
	}

	/// <summary>
	/// sets inBounds to whether or not vehicle is in bounds.
	/// </summary>
	private void CheckBoundaries() {
		if (position.x < -18 || position.x > 18 || position.z > 18 || position.z < -18)
		{
			inBounds = false;
		}
		else {
			inBounds = true;
		}
	}

	public abstract void CalcSteeringForces();

	// Obstacle Avoidance

	public Vector3 AvoidObstacle(Obstacle toAvoid) {
		
		return AvoidObstacle(toAvoid.transform.position, toAvoid.GetComponent<CapsuleCollider>().radius);
	}

	public Vector3 AvoidObstacle(Vector3 targetPosition, float otherRadius)
	{
		Vector3 meToOther = targetPosition - position;
		float fwdMeToOtherDot = Vector3.Dot(transform.forward, meToOther);

		//object is behind vehicle
		if (fwdMeToOtherDot < 0)
		{
			return Vector3.zero;
		}

		//too far to left or right
		float rightMeToOtherDot = Vector3.Dot(transform.right, meToOther);
		if (Mathf.Abs(rightMeToOtherDot) > otherRadius + GetComponent<Renderer>().bounds.extents.magnitude/2)
		{
			return Vector3.zero;
		}

		//too far away
		float distance = meToOther.magnitude - otherRadius;
		if (distance > safeDistance)
		{
			return Vector3.zero;
		}

		float weight = 0;
		if (distance <= 0)
		{
			weight = float.MaxValue;
		}
		else
		{
			weight = Mathf.Pow(safeDistance / distance, 2f);
		}

		weight = Mathf.Min(weight, 100000);

		Vector3 desiredVelocity = Vector3.zero;

		//if obstacle is on left, steer right

		if (rightMeToOtherDot < 0)
		{
			desiredVelocity = transform.right * maxSpeed;
		}
		else
		{
			desiredVelocity = transform.right * -maxSpeed;
		}
		Vector3 steeringForce = (desiredVelocity - velocity) * weight;

		return steeringForce;
	}

	

	public Vector3 GetFuturePosition(float x) {
		return position + x*velocity;
	}

	public Vector3 Wander() {
		Vector3 steeringForce = Vector3.zero;

		if (velocity == Vector3.zero)
		{
			wanderCircleLocation = new Vector3(UnityEngine.Random.Range(-AgentManager.instance.floorWidth, AgentManager.instance.floorWidth), 1,
											   UnityEngine.Random.Range(-AgentManager.instance.floorHeight, AgentManager.instance.floorHeight));
		}
		else {
			wanderCircleLocation = GetFuturePosition(3f);
		}

		wanderCircleAngle = UnityEngine.Random.Range(0, 360);
		wanderLocation = new Vector3(wanderCircleLocation.x + Mathf.Cos(wanderCircleAngle) * wanderCircleRadius, 1,
										wanderCircleLocation.z + Mathf.Sin(wanderCircleAngle) * wanderCircleRadius);

		steeringForce = Seek(wanderLocation);
		return steeringForce;
	}

	protected Vector3 Separate(Vector3 targetPosition, float desiredDistance)
	{
		// Calculate distance to the other object
		float distanceToTarget = Vector3.Distance(position, targetPosition);

		// if the distance is basically 0, then it's probably me'
		if (distanceToTarget <= float.Epsilon)
		{
			return Vector3.zero;
		}

		// Flee away from the other object
		Vector3 fleeForce = Flee(targetPosition);

		// Scale the force based on how close I am
		fleeForce = fleeForce.normalized * Mathf.Pow(desiredDistance / distanceToTarget, 2);

		// Draw that force
		//Debug.DrawLine(position, position + fleeForce, Color.cyan);
		return fleeForce;
	}



	void OnRenderObject()
		{

		if (AgentManager.instance.linesOn) {
			//draws forward debug line
			forwardLine.SetPass(0);
			GL.Begin(GL.LINES);
			GL.Vertex(transform.position);
			GL.Vertex(transform.position + transform.forward);
			GL.End();

			//draws right debug line
			rightLine.SetPass(0);
			GL.Begin(GL.LINES);
			GL.Vertex(position);
			GL.Vertex(transform.position + transform.right);
			GL.End();

			//if the human/zombie has a target, draws line to target
			if (target != null)
			{
				GL.Begin(GL.LINES);
				chasingLine.SetPass(0);
				GL.Vertex(transform.position);
				GL.Vertex(target.transform.position);
				GL.End();
			}
		}
	}



#if UNITY_EDITOR
	private void OnValidate()
	{
		// Make sure that mass isn't set to 0
		mass = Mathf.Max(mass, 0.0001f);
	}
#endif
}
