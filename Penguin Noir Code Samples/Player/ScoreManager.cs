using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum for trick names
/// </summary>
public enum TrickNames
{
    None,
    Icicle,
    Pearl,
    BouncePad,
    Flipper,
    WaterTube,
    GunShot,
    Seal,
    Prop
}

public class ScoreManager : MonoBehaviour
{
    private float notMovingCooldown = 0;
    private float maxSlideCooldown = 0;
    private int counter = 0;
    private float totalTrickScore;
    TrickNames lastTrick;
    bool lockDownScore = false;     //Sets it so that score cannot be added or score can be added
    [SerializeField] GameObject floatingText;
    Penguin player;
    ScoreUI scoreUI;
    CurrentComboUI currentComboUI;
    CurrentComboTimerUI currentComboTimerUI;
    CinemachineImpulseSource cinemachineImpulseSource;
    PlayerCollectible playerCollectible;


    // getter for singleton usage
    static private ScoreManager instance;
    public static ScoreManager Instance
    {
        get
        {
            return instance;
        }
    }

    public float TotalTrickScore
        { get { return totalTrickScore; }}

    private float currentComboScore;
    public float CurrentComboScore
        { get { return currentComboScore; }}

    private float currentMultiplier = 1;
    public float CurrentMultiplier
        { get { return currentMultiplier; }}

    // set instance so it may be called by other scripts
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("ScoreManager - Start:: Attempted to assign 2 instances of score manager!");
            Destroy(this.gameObject);
        }
    }

    void Update()
    {
        //Debug.Log("Time:" + Time.timeScale);
        if (!player)
        {
            try { player = GameObject.FindGameObjectsWithTag("Penguin")[0].GetComponent<Penguin>(); }
            catch { }
        }
        else if (!scoreUI)
        {
            try { scoreUI = GameObject.FindGameObjectWithTag("LevelComplete").GetComponent<ScoreUI>(); }
            catch { }
        }
        else if (!currentComboUI)
        {
            try { currentComboUI = GameObject.FindGameObjectWithTag("CurrentComboUI").GetComponent<CurrentComboUI>(); }
            catch { }
        }
        else if(!currentComboTimerUI)
        {
            try { currentComboTimerUI = GameObject.FindGameObjectWithTag("CurrentComboTimerUI").GetComponent<CurrentComboTimerUI>();}
            catch { }
        }
        else if (!playerCollectible)
        {
            try { playerCollectible = GameObject.FindGameObjectWithTag("CollectibleUI").GetComponent<PlayerCollectible>(); }
            catch { }
        }
        else if (!lockDownScore)
        {
            if (currentComboScore > 0 || currentMultiplier > 1)
            {
                CheckNoPointsAwarded();
                currentComboTimerUI.ShowCurrentComboTimer();
            }
            else
            {
                currentComboTimerUI.TimerCanvasVisable();
            }
        }
    }

    /// <summary>
    /// Increases the player's current combo score and current combo mulitplier
    /// </summary>
    /// <param name="points"></param>
    /// <param name="multiplier"></param>
    public void IncreaseScore(float points, TrickNames trick)
    {
        if(!lockDownScore)
        {
            //Increases values
            if (lastTrick != trick)
            {
                currentMultiplier += trick == TrickNames.GunShot ? 0f : 1f;
                lastTrick = trick == TrickNames.GunShot ? lastTrick : trick;
                currentComboUI.ShowCurrentMultiplier();
                AudioManager.Instance.Play(Sounds.MultiplierAdded);

                // if new highest combo, update data
                if (currentMultiplier >= DataTracker.Instance.highestComboMultiplier)
                {
                    DataTracker.Instance.highestComboMultiplier = currentMultiplier;
                }
            }
            currentComboScore += points;
            currentComboUI.ShowCurrentCombo();
            notMovingCooldown = 0;
            currentComboTimerUI.resetTimer(false);

            // Play sound
            AudioManager.Instance.Play(Sounds.PointsAdded);
        }
    }

    /// <summary>
    /// Creates the floating text object
    /// </summary>
    public void SpawnFloatingPoints(float points)
    {
        if (!lockDownScore)
        {
            //Instantiates floating text
            GameObject currentText = Instantiate(floatingText, player.gameObject.transform.position, transform.rotation);
            currentText.GetComponent<FloatingPoints>().SetText(points);
        }
    }

    /// <summary>
    /// Sets current multiplier to 1
    /// </summary>
    public void ResetMultiplier()
    {
        if(!lockDownScore)
        {
            currentMultiplier = 1f;
            notMovingCooldown = 0f;
            currentComboTimerUI.resetTimer(true);

        }
    }

    /// <summary>
    /// Sets current combo score to 0
    /// </summary>
    public void ResetScore()
    {
        if(!lockDownScore)
        {
            currentComboScore = 0f;
            notMovingCooldown = 0f;
            
        }
    }

    /// <summary>
    /// Checks to see if the player has earned any points or multiplier
    /// </summary>
    private void CheckNoPointsAwarded()
    {
        if (!lockDownScore)
        {
            if (DialogManager.isActive)
            {

            }
            else
            {
                notMovingCooldown += Time.deltaTime;
                currentComboTimerUI.DecreaseTime();
                if (notMovingCooldown >= MonoBehaviourSingletonPersistent<Constants>.Instance.bankScoreTime)
                {
                    EndTrick();
                }
            }
        }
    }

    /// <summary>
    /// Ends the player's current trick and resets timers and variables to 0
    /// </summary>
    public void EndTrick()
    {
        if(!lockDownScore && currentComboUI)
        {
            //Play Audio
            AudioManager.Instance.Play(Sounds.ComboBreak);
            currentComboUI.FadeScoreCanvas();

            //Shakes Camera
            cinemachineImpulseSource.GenerateImpulse(MonoBehaviourSingletonPersistent<Constants>.Instance.globalShakeForce);
            //Resets variables
            lastTrick = TrickNames.None;
            currentComboScore = 0f;
            currentMultiplier = 1f;
            notMovingCooldown = 0f;
            maxSlideCooldown = 0f;

            currentComboTimerUI.resetTimer(false);
            currentComboTimerUI.TimerCanvasVisable();
            DataTracker.Instance.numberOfCombos++;
            
        }
    }

    /// <summary>
    /// End the player's current trick but turns the text green instead of red
    /// </summary>
    public void BankScore()
    {
        if (currentComboScore != 0 || CheckForUnbankedPearl())
        {
            currentComboUI.BankScore();
            totalTrickScore += currentComboScore * currentMultiplier;
            scoreUI.UpdateScore();

            //Shakes Camera
            cinemachineImpulseSource.GenerateImpulse(MonoBehaviourSingletonPersistent<Constants>.Instance.globalShakeForce * .2f);

            AudioManager.Instance.Play(Sounds.PointsBanked);

            DataTracker.Instance.numberOfBanks++;
            DataTracker.Instance.numberOfCombos++;
        }

        lastTrick = TrickNames.None;
        currentComboScore = 0f;
        currentMultiplier = 1f;
        notMovingCooldown = 0f;
        maxSlideCooldown = 0f;
        currentComboTimerUI.resetTimer(false);
        currentComboTimerUI.TimerCanvasVisable();
    }

    /// <summary>
    /// Checks to see if player has a pearl before banking
    /// </summary>
    /// <returns></returns>
    public bool CheckForUnbankedPearl()
    {
        //return playerCollectible.CollectibleStatus.Length != 0;
        int countUnbankedPearls = 0;
        for (int i = 0; i < playerCollectible.CollectibleStatus.Length; ++i)
        {
            if (playerCollectible.CollectibleStatus[i] == 1)
                return true;
        }
        return false;
    }

    public float AddScore(long incomingScore)
    {
         totalTrickScore = incomingScore;
         return Mathf.Round(totalTrickScore);
    }

    /// <summary>
    /// False disables score, true enables score
    /// </summary>
    /// <param name="enable"></param>
    public void LockScore(bool enable)
    {
        EndTrick();

        lockDownScore = enable;

        if(!currentComboUI)
        {
            currentComboUI = GameObject.FindGameObjectWithTag("CurrentComboUI").GetComponent<CurrentComboUI>();
        }
        if(!currentComboTimerUI)
        {
            currentComboTimerUI = GameObject.FindGameObjectWithTag("CurrentComboTimerUI").GetComponent<CurrentComboTimerUI>();
        }
        currentComboUI.DisableScoreCanvas(!enable);
        currentComboTimerUI.TimerCanvasVisable();
        currentComboTimerUI.DisableTimerCanvas(!enable);
    }

    /// <summary>
    /// This is called when level ends, banks current score and prevents more score buildup
    /// </summary>
    public void EndLevel()
    {
        BankScore();

        lockDownScore = true;
        currentComboUI.DisableScoreCanvas(true);
        currentComboTimerUI.DisableTimerCanvas(true);
        currentComboTimerUI.TimerCanvasVisable();
    }

    /// <summary>
    /// Flushes score so that the next level doesn't continue with your current score
    /// </summary>
    public void FlushScore()
    {
        totalTrickScore = 0f;
        currentMultiplier = 1f;
        currentComboScore = 0f;
    }
}
