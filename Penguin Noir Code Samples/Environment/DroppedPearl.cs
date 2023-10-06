using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedPearl : MonoBehaviour
{

    private bool spawning;
    private bool flying;
    private Vector3 velocity;
    private GameObject clam;
    private int clamIndex;
    float t;

    public PlayerCollectible pc;

    // Start is called before the first frame update
    void Start()
    {
        spawning = false;
        flying = false;
        t = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (spawning)
        {
            transform.position += velocity * Time.fixedDeltaTime;
            velocity *= 0.94f;
            if(velocity.magnitude < 0.02f)
                spawning = false;
        }
        else if (flying)
        {
            t += Time.fixedDeltaTime / 3f;
            transform.position = Vector3.Lerp(transform.position, clam.transform.position,t);
            if (t >= 0.15f)
            {
                pc.PearlSetup(clamIndex);
                Destroy(gameObject);
            }
                
        }
        else
        {
            Invoke("FlyAway", 0f);
        }
    }

    public void Spawn(Vector3 vel, GameObject clam, int i)
    {
        velocity = vel;
        this.clam = clam;
        clamIndex = i;
    }

    public void FlyAway()
    {
        flying = true;
    }

}
