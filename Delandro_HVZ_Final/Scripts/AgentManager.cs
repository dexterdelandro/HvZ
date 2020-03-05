using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
	public static AgentManager instance;

	public Obstacle obstaclePrefab;
	public List<Obstacle> obstacles;
	public int numObstacles;
	public Obstacle obstacleTemp;

	public ZombieScript zombiePrefab;
	public List<ZombieScript> zombies;
	public int numZombies;
	public ZombieScript zombieTemp;

	public HumanScript humanPrefab;
	public List<HumanScript> humans;
	public int numHumans;
	public HumanScript humanTemp;

	//public GameObject zombie;
	//public GameObject human;
	private float targetDistance;
	private Vehicle target;
	private float tempDistance;
	//public List<GameObject> zombies;
	//public List<GameObject> humans;
	public float floorWidth = 18f;
	public float floorHeight = 18f;
	//private GameObject temp;

	//private HumanScript humanScript;
	//private ZombieScript zombieScript;

	public bool linesOn = true;

	// Start is called before the first frame update
	void Start()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(this);
		}

		zombies = new List<ZombieScript>();
		humans = new List<HumanScript>();
		obstacles = new List<Obstacle>();

		//creates obstacles at random positions
		for (int i = 0; i < numObstacles; i++)
		{
			obstacleTemp = Instantiate(obstaclePrefab);
			obstacleTemp.transform.position = new Vector3(Random.Range(-floorWidth, floorWidth), 3, Random.Range(-floorHeight, floorHeight));
			obstacles.Add(obstacleTemp);
		}

		//cerates humans at random positions
		for (int i = 0; i < numHumans; i++)
		{
			humanTemp = Instantiate(humanPrefab);
			humanTemp.transform.position = new Vector3(Random.Range(-floorWidth, floorWidth), 1, Random.Range(-floorHeight, floorHeight));
			humans.Add(humanTemp);
		}

		//creates zombies at random positions
		for (int i = 0; i < numZombies; i++)
		{
			zombieTemp = Instantiate(zombiePrefab);
			zombieTemp.transform.position = new Vector3(Random.Range(-floorWidth, floorWidth), 1, Random.Range(-floorHeight, floorHeight));
			zombies.Add(zombieTemp);
		}
	}

	// Update is called once per frame
	void Update()
	{
		//assign the closest zombie to each human
		AssignHumanTargets();

		//assign the closest human to each zombie
		AssignZombieTargets();

		UpdateLines();

	}

	public void AssignHumanTargets()
	{
		for (int i = 0; i < humans.Count; i++)
		{
			//finds the zombie that is closest to the human
			target = zombies[0];
			Mathf.Abs(targetDistance = Vector3.Distance(humans[i].transform.position, target.transform.position));
			tempDistance = targetDistance;
			for (int j = 0; j < zombies.Count; j++)
			{
				tempDistance = Mathf.Abs(Vector3.Distance(humans[i].transform.position, zombies[j].transform.position));
				if (tempDistance < targetDistance)
				{
					targetDistance = tempDistance;
					target = zombies[j];
				}
			}
			//sets the human's target to the closest zombie
			humans[i].target = target;
			humans[i].targetDistance = targetDistance;
			
		}
	}

	public void AssignZombieTargets()
	{
		if (humans.Count > 0)
		{
			for (int i = 0; i < zombies.Count; i++)
			{

				target = humans[0];
				Mathf.Abs(targetDistance = Vector3.Distance(zombies[i].transform.position, target.transform.position));
				tempDistance = targetDistance;
				for (int j = 0; j < humans.Count; j++)
				{
					tempDistance = Mathf.Abs(Vector3.Distance(zombies[i].transform.position, humans[j].transform.position));
					if (tempDistance < targetDistance)
					{
						targetDistance = tempDistance;
						target = humans[j];
					}
				}
				//sets the human target to the zombie
				zombies[i].target = target;

				//zombie is touching the human so turn human into the zombie
				if (targetDistance <= 1)
				{
					
					humans.Remove((HumanScript)target);
					zombieTemp = Instantiate(zombiePrefab);
					zombieTemp.transform.position = target.transform.position;
					zombies.Add(zombieTemp);
					target.gameObject.SetActive(false);
					target.humanFuturePositionPrefab.gameObject.SetActive(false);
					target.zombieFuturePositionPrefab.gameObject.SetActive(false);
					break;
				}
			}

		}
		else
		{
			//make the zombies stop moving when all humans are gone
			for (int i = 0; i < zombies.Count; i++)
			{
				zombies[i].target = null;
				
			}

		}
	}

	private void UpdateLines()
	{
		if (Input.GetKeyDown(KeyCode.D)) linesOn = !linesOn;
	}
}
