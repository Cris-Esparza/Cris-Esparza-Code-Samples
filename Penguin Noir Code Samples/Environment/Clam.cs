using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clam : MonoBehaviour
{

    [SerializeField] private GameObject pearlPrefab;
    [SerializeField] private Sprite clamShellOpen;
    [SerializeField] private Sprite closedShell;
    [SerializeField] private GameObject pearl;
    [SerializeField] private GameObject pearlUI;

    //Shell variables
    SpriteRenderer shellSpriteRenderer;

    //Pearl variables
    Animator collectibleAnimator;
    SpriteRenderer pearlSpriteRenderer;
    Collectible collectible;

    // Start is called before the first frame update
    void Start()
    {
        shellSpriteRenderer = GetComponent<SpriteRenderer>();
        collectible = pearl.GetComponent<Collectible>();
        collectibleAnimator = pearl.GetComponent<Animator>();
        pearlSpriteRenderer = pearl.GetComponent <SpriteRenderer>();
        
    }

    public GameObject Respawn()
    {
        GetComponent<SpriteRenderer>().sprite = clamShellOpen;
        pearl = Instantiate(pearlPrefab,transform.position,Quaternion.identity);
        pearl.transform.parent = transform;
        collectible = pearl.GetComponent<Collectible>();
        collectible.shell = gameObject;
        collectibleAnimator = pearl.GetComponent<Animator>();
        pearlSpriteRenderer = pearl.GetComponent<SpriteRenderer>();
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        return pearl;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Penguin"))
        {
            if(collision.gameObject.GetComponent<Penguin>() != null)
            {
                if(!collision.gameObject.GetComponent<Penguin>().isUnableToCollectPearls)
                {
                    // play sound, change pitch based on collectible number
                    AudioManager.Instance.Play(Sounds.CollectablePickup);
                    AudioManager.Instance.SetPitch(Sounds.CollectablePickup, 1 + (0.18921f)); // musical interval of a minor 3rd between each collectable sound
                    AudioManager.Instance.Play(Sounds.SquawkCollectable);
                    //call change sprite method
                    shellSpriteRenderer.sprite = closedShell;
                    gameObject.GetComponent<CircleCollider2D>().enabled = false;
                    Instantiate(pearlUI);
                    collectibleAnimator.SetTrigger("isCollected");
                    pearlSpriteRenderer.sortingLayerName = "PearlSwirl";
                    //shrinks worldspace pearl so animation-tied logic doesn't break
                    pearlSpriteRenderer.transform.localScale = Vector3.zero;
                    collectible.isCollected = true;
                    collectible.blackHoleSparkle.Stop();
                    collectible.blackHoleSparkle.Clear(false);
                    //disables particle effects in worldspace pearl

                    Transform[] allChildren = collectible.GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChildren)
                    {
                        child.gameObject.SetActive(false);
                    }

                    //Awards points
                    MonoBehaviourSingletonPersistent<ScoreManager>.Instance.IncreaseScore(MonoBehaviourSingletonPersistent<Constants>.Instance.pearlPoints, TrickNames.Pearl);
                    MonoBehaviourSingletonPersistent<ScoreManager>.Instance.SpawnFloatingPoints(MonoBehaviourSingletonPersistent<Constants>.Instance.pearlPoints);

                }
            }
        }
    }
}
