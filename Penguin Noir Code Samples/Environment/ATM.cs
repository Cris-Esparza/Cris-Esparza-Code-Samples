using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATM : CollectibleSubject
{

    ScoreManager scoreManager;
    ParticleSystem moneyParticles;

    // Start is called before the first frame update
    void Start()
    {
        scoreManager = MonoBehaviourSingletonPersistent<ScoreManager>.Instance;
        moneyParticles = GetComponentInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player hits us
        if(collision.tag == "Penguin" && (scoreManager.CurrentComboScore != 0 || scoreManager.CheckForUnbankedPearl()))
        {
            scoreManager.BankScore();
            moneyParticles.Play();
            NotifyObservers();
        }
    }
}
