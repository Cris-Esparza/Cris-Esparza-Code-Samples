using Powder.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Powder.Singleton
{
    public class BoulderSpawner : MonoBehaviour
    {
        private Vector3 playerPos;
        public GameObject boulderPrefab;
        public GameObject impactParticles;
        public float minTime;
        public float maxTime;
        public float spawnSide;
        private float spawnTime;

        private void Start()
        {
            spawnTime = 1f;
            StartCoroutine(SpawnBoulder(spawnTime));
        }

        IEnumerator SpawnBoulder(float spawnTime)
        {
            yield return new WaitForSeconds(1f);

            while (true)
            {
                playerPos = GameManager.Instance.player.snowballs[0].transform.position;
                float xPos;
                float yPos = 6;
                float zPos = playerPos.z + 10;
                Vector3 spawnPos;
                spawnSide = Random.Range(0, 10);
                spawnTime = Random.Range(minTime, maxTime);

                // randomly decide which side to spawn on
                if (spawnSide < 5)
                {
                    xPos = 10;
                }
                else
                {
                    xPos = 34;
                }

                spawnPos = new Vector3(xPos, yPos, zPos);
                GameObject boulder = Instantiate(boulderPrefab, spawnPos, Quaternion.identity);

                // determine which side boulder is spawning
                if (spawnSide < 5)
                {
                    boulder.GetComponent<Boulder>().SetDirection(new Vector3(0.5f, 0, 0.5f));
                }
                else
                {
                    boulder.GetComponent<Boulder>().SetDirection(new Vector3(-0.5f, 0, 0.5f));
                }

                yield return new WaitForSeconds(spawnTime);
            }
        }
    }
}

