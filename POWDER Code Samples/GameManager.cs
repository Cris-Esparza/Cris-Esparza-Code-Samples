using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Powder.Singleton
{
    public class GameManager : Singleton<GameManager>
    {
        public GameObject[] section;
        public GameObject playerPrefab, boulderSpawnPrefab, splitUI, pauseUI, gameOverUI;
        public Player player;
        public BoulderSpawner boulderSpawner;
        public int zPos = 0;
        public bool creatingSection = false;
        public int secNum;
        public bool isReloading = false;

        void Start()
        {
            StartCoroutine(GenerateSection());
            CreatePlayer();
            BoulderSpawner();
        }

        void Update()
        {
            if (player != null)
            {
                if (player.isAlive)
                {
                    // checking if level is generating sections
                    if (!creatingSection)
                    {
                        creatingSection = true;
                        StartCoroutine(GenerateSection());
                    }
                    gameOverUI.SetActive(false);
                }
                else if (!isReloading)
                {
                    zPos = 0;
                    isReloading = true;
                }
                else if (!player.isAlive)
                {
                    gameOverUI.SetActive(true);
                }
            }

            // pause menu functionality
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }

        IEnumerator GenerateSection()
        {
            secNum = Random.Range(0, section.Length);
            Instantiate(section[secNum], new Vector3(0, 0, zPos), Quaternion.identity);
            zPos += 99;
            yield return new WaitForSeconds(3);
            creatingSection = false;  
        }

        void CreatePlayer()
        {
            player = Instantiate(playerPrefab.GetComponent<Player>());
        }

        void BoulderSpawner()
        {
            boulderSpawner = Instantiate(boulderSpawnPrefab.GetComponent<BoulderSpawner>());
        }

        IEnumerator LevelRestart()
        {
            //yield return new WaitForSeconds(2);
            gameOverUI.SetActive(false);
            SceneManager.LoadScene(1);
            while (GameObject.FindGameObjectWithTag("Ground") != null)
            {
                yield return new WaitForSeconds(.5f);
            }
            CreatePlayer();
            BoulderSpawner();
        }


        public void Restart()
        {
            StartCoroutine(LevelRestart());
        }

        public void PauseGame()
        {
            Time.timeScale = 0;
            for (int i = 0; i < player.snowballs.Count; i++)
            {
                player.snowballs[i].GetComponent<Snowball>().movementSound.Pause();
            }
            pauseUI.SetActive(true);
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
            for (int i = 0; i < player.snowballs.Count; i++)
            {
                player.snowballs[i].GetComponent<Snowball>().movementSound.Play();
            }
            pauseUI.SetActive(false);
        }

        public void QuitGame()
        {
            PlayerPrefs.SetInt("bestScore", 0);
            Application.Quit();
        }
    }
}



