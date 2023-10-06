using Powder.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplitBar : MonoBehaviour
{
    public Image splitBar;
    public Player player;
    public GameObject leftParticlePrefab, rightParticlePrefab;
    public GameObject leftParticles, rightParticles;
    private float sizeDiff;
    private bool canEmit;

    private void Awake()
    {   
        canEmit = false;
    }

    void FixedUpdate()
    {
        sizeDiff = GameManager.Instance.player.resetSize.x - GameManager.Instance.player.snowballSize.localScale.x;
        splitBar.fillAmount = 1 - sizeDiff;
        if (splitBar.fillAmount == 1 && canEmit == false)
        {
            canEmit = true;
            // activate particle systems once bar isfull
            PlayParticles();
        }
        if (splitBar.fillAmount < 1 && canEmit == true)
        {
            DestroyParticles();
            canEmit = false;
        }
    }

    private void PlayParticles()
    {
        leftParticles = Instantiate(leftParticlePrefab);
        rightParticles = Instantiate(rightParticlePrefab);
    }

    private void DestroyParticles()
    {
        Destroy(leftParticles);
        Destroy(rightParticles);
    }
}
