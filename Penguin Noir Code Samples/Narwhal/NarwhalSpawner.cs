using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Build.Content;
#endif
using UnityEngine;

public class NarwhalSpawner : NarwhalObserver
{
    // saved for efficiency
    [SerializeField]
    GameObject narwhal;     //Store narwhal to call
    [SerializeField]
    NarwhalRoute route; //Set the path the narwhal has to follow
    [SerializeField]
    float narwhalSpeed = 35f; //Allows you to set the speed of each spawn point individually
    private bool spawned = true;       //Sets the flag to prevent respawning
    private float spawnTimer;
    [SerializeField] private Transform spawnPoint;

    private bool isVisible = false;
    private bool awaitingSpawn = false;

    private void Awake()
    {
        spawnTimer = MonoBehaviourSingletonPersistent<Constants>.Instance.narwhalSpawnTimer;
    }

    public void Start()
    {
        StartCoroutine(SpawnNarwhal());
        spawned = true;
    }

    private void Update()
    {
        Vector3 relativePos = Camera.main.WorldToViewportPoint(transform.position);
        if ((relativePos.x > 0 && relativePos.x < 1) && (relativePos.y > 0 && relativePos.y < 1))
        {
            isVisible = true;
        }
        else
        {
            isVisible = false;
        }
    }

    /// <summary>
    /// Spawns the narwhal at the same angle this object is moving
    /// </summary>
    IEnumerator SpawnNarwhal()
    {
        awaitingSpawn = true;

        yield return new WaitForSeconds(spawnTimer);

        awaitingSpawn = false;
        GameObject gameObject = Instantiate(narwhal, spawnPoint.transform.position, 
                                            Quaternion.Euler(0, 0, this.gameObject.transform.rotation.z));
        gameObject.GetComponent<Narwhal>().SetPoints(route.Points);
        gameObject.GetComponent<Narwhal>().SetSpawner(this);
        gameObject.GetComponent<Narwhal>().SetSpeed(narwhalSpeed);

		if (isVisible)
		{
			AudioManager.Instance.Play(Sounds.NarwhalRumble);
			AudioManager.Instance.Play(Sounds.NarwhalBreak);
		}
	}

    /// <summary>
    /// When this observer is notified a new narwhal can spawn
    /// </summary>
    public override void Notify(NarwhalSubject subject)
    {
        StartCoroutine(SpawnNarwhal());
        spawned = true;
    }
}
