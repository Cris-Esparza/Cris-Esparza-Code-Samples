using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Powder.Singleton
{
    public class LevelDistance : Singleton<LevelDistance>
    {
        public GameObject distanceDisplay;
        public GameObject highScoreDisplay, gameOverHighScore;
        public float distanceFloat = 0;
        public int distanceRun = 0;
        public int highScore = 0;
        public float snowballCount;


        void Awake()
        {
            PlayerPrefs.DeleteAll();
        }

        void Start()
        {
            GetBestScore();
            CheckIfPrefsSet();
        }

        void Update()
        {
            snowballCount = GameManager.Instance.player.snowballs.Count;

            if (snowballCount >= 1)
            {
                distanceFloat += snowballCount * Time.deltaTime;
                distanceRun = (int)distanceFloat;
                distanceDisplay.GetComponent<Text>().text = "" + distanceRun;
                CheckBestScore();
                highScoreDisplay.GetComponent<Text>().text = "" + highScore;
                gameOverHighScore.GetComponent<Text>().text = "" + highScore;
            }
            else
            {
                distanceFloat = 0f;
            }
        }

        private void SetBestScore()
        {
            PlayerPrefs.SetInt("bestScore", highScore);
        }


        private void CheckBestScore()
        {
            bool bestScore = false;

            if (distanceRun >= highScore)
            {
                bestScore = true;
                highScore = distanceRun;
            }

            if (bestScore)
            {
                SetBestScore();
            }
        }

        private void GetBestScore()
        {
            if (PlayerPrefs.HasKey("bestScore"))
            {
                highScore = PlayerPrefs.GetInt("bestScore");
            }
        }

        private void CheckIfPrefsSet()
        {
            if (!PlayerPrefs.HasKey("bestScore"))
            {
                PlayerPrefs.SetInt("bestScore", 0);
            }
        }
    }
}


