using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;

/// <summary>
/// Handles the UI update when a collectible is collected
/// </summary>
public class PlayerCollectible : CollectibleObserver
{
    [SerializeField]
    private Sprite collectibleSprite;
    [SerializeField]
    private Sprite emptyCollectibleSprite;
    [SerializeField]
    private Sprite closedCollectibleSprite;

    private Sprite[] collectibleSprites;

    [SerializeField]
    private SpriteRenderer[] collectibleRenderers;
    [SerializeField]
    private Animator[] collectibleAnimators;
    [SerializeField]
    private ParticleSystem[] collectibleParticles;
    [SerializeField]
    private GameObject[] collectibleCheck;
    [SerializeField]
    private GameObject[] UIPearlFallObjects;

    [SerializeField]
    private GameObject pearlPrefab;
    [SerializeField]
    private GameObject pearlUIPrefab;

    private GameObject[] clams;

    Collectible collectible;
    int[] collectibleStatus; //tracks number of collectibles you have
    public int[] CollectibleStatus
    {
        get { return collectibleStatus; }
    }
    bool banked3;
    // for the gradual change in color after losing pearls
    float c_change;

    // used for lost pearl UI animation
    private bool droppingPearl;
    private bool hasPearl;

    Penguin player;

    public bool getDroppingPearl { get { return droppingPearl;} }

    // Start is called before the first frame update
    void Start()
    {
        // gets a list of game objects with this tag
        GameObject[] atms = GameObject.FindGameObjectsWithTag("ATM");
        // loops through the list adding this script as a observer to the subject
        foreach (GameObject gameObject in atms)
        {
            if (gameObject)
            {
                // checks if the game object exists and attachs it to the Observer
                ATM atm = gameObject.GetComponent<ATM>();
                if (atm != null)
                {
                    atm.Attach(this);
                }
            }
        }

        // gets a list of game objects with this tag
        GameObject[] collect = GameObject.FindGameObjectsWithTag("Collectible");
        // loops through the list adding this script as a observer to the subject
        foreach (GameObject gameObject in collect)
        {
            if (gameObject)
            {
                // checks if the game object exists and attachs it to the Observer
                collectible = gameObject.GetComponent<Collectible>();
            }

            if (collectible != null)
            {
                collectible.Attach(this);
            }
        }

        player = GameObject.FindGameObjectsWithTag("Penguin")[0].GetComponent<Penguin>();

        collectibleStatus = new int[3] { 0,0,0 };
        banked3 = false;

        c_change = 1;

        clams = new GameObject[3];
        collectibleSprites = new Sprite[3] { emptyCollectibleSprite, collectibleSprite, closedCollectibleSprite };

        // sets all of the Stars to empty
        foreach (SpriteRenderer s in collectibleRenderers)
        {
            s.sprite = emptyCollectibleSprite;
        }

        Invoke("SetupClams", 0.01f);

        hasPearl = false;
        droppingPearl = false;
    }

    private void SetupClams()
    {
        GameObject pearlParent = GameObject.FindGameObjectWithTag("PearlParent");
        for (int i = 0; i < 3; i++)
        {
            clams[i] = pearlParent.transform.GetChild(i).gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Checks if all three collectibles have been collected to finish the level.
        if (banked3)
        {
            player.LevelCompleted();
        }
        for (int i = 0; i < 3; i++)
        {
            if (c_change < 1)
            {
                c_change += .005f;
                collectibleRenderers[i].color = new Color(1, c_change, c_change, 1);
            }
        }
    }
    /// <summary>
    /// Searches the tags to know which ui element to highlight
    /// </summary>
    /// <param name="subject"></param>
    public override void Notify(CollectibleSubject subject)
    {
        if (subject is Collectible)
        {
            //tracks number of collectibles you have
            for (int i = 0; i < 3; i++)
                if(subject.transform.parent.gameObject == clams[i])
                {
                    collectibleStatus[i] = 1;
                    collectibleAnimators[i].Play("clamRock");
                    collectibleParticles[i].Play();

                }

            hasPearl = true;
            droppingPearl = false;
        }

        if(subject is ATM)
        {
            if (hasPearl)
            {
                hasPearl = false;
                
            }

            int c = 0;
            //goes through and populates all the slots it needs to for the UI
            for (int i = 0; i < 3; i++)
            {
                if(collectibleStatus[i] == 1)
                {
                    collectibleStatus[i] = 2;
                }
                if (collectibleStatus[i] == 2)
                {
                    c++;
                    collectibleCheck[i].SetActive(true);
                    collectibleParticles[i].Stop();
                    collectibleParticles[i].Clear();
                    collectibleAnimators[i].Play("clamPulse");
                }
                    
            }
            if(c >= 3)
                banked3 = true;
        }

        for(int i = 0; i < 3; i++)
        {
            collectibleRenderers[i].sprite = collectibleSprites[collectibleStatus[i]];
            
        }
    }

    public void RemovePearls()
    {

        if (hasPearl)
        {
            droppingPearl = true;
            hasPearl = false;
        }


        //Debug.Log(player.transform.position);
        for (int i = 0; i < 3; i++)
        {
            if (collectibleStatus[i] == 1)
            {
                collectibleRenderers[i].sprite = emptyCollectibleSprite;
                collectibleRenderers[i].color = new Color(1, 0, 0, 1);
                c_change = 0;
                UIPearlFallObjects[i].SetActive(true);
                Vector3 offset = Random.insideUnitCircle.normalized * 3;
                offset.y = offset.y < 0 ? -offset.y : offset.y;
                offset.z = 0;
                GameObject pearl = Instantiate(pearlPrefab, player.transform.position, Quaternion.identity);
                DroppedPearl d = pearl.GetComponent<DroppedPearl>();
                //Debug.Log("RemovePearls:: d == " + d);
                d.Spawn(offset, clams[i],i);
                d.pc = this;
                collectibleStatus[i] = 0;
                collectibleAnimators[i].Play("Idle");
                collectibleParticles[i].Stop();
                collectibleParticles[i].Clear();
            }
        }
    }

    public void PearlSetup(int i)
    {
        GameObject pearl = clams[i].GetComponent<Clam>().Respawn();
        pearl.GetComponent<Collectible>().Attach(this);
    }

}
