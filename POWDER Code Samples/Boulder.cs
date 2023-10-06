using Powder.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    public BoulderSpawner boulderSpawner;
    public AudioSource rumble;
    public GameObject boulderParticlePrefab;
    public float speed;
    public float lifeTime;
    public bool isDestroyed;
    private float spawnSide;
    private Vector3 direction;
    private ParticleSystem particleSystem;

    // Start is called before the first frame update
    void Start()
    {
        isDestroyed = false;
        if (rumble != null)
        {
            rumble.Play();
        }
    }

    public void SetDirection(Vector3 direction)
    { this.direction = direction; }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = transform.position + direction * speed * Time.deltaTime;
        lifeTime = lifeTime - Time.deltaTime;
        if (lifeTime <= 0 )
        {
            isDestroyed = DestroyBoulder();
            StartCoroutine(DestroyBoulderCoroutine());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Snowball")
        {
            StartCoroutine(DestroyBoulderCoroutine());
        }
    }

    bool DestroyBoulder()
    {
        isDestroyed = true;
        if (rumble != null)
        {
            rumble.Stop();
        }
        return isDestroyed;
    }
    
    IEnumerator DestroyBoulderCoroutine()
    {
        particleSystem = Instantiate(boulderParticlePrefab, gameObject.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        yield return new WaitForSeconds(.1f);
        Destroy(gameObject);
    }
}
