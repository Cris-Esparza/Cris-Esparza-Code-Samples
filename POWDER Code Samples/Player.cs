using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Powder.Singleton
{
    public class Player : MonoBehaviour
    {
        public List<GameObject> snowballs = new List<GameObject>();
        public Vector3 resetSize = new Vector3(2.5f, 2.5f, 2.5f);
        private Vector3 startingPos = new Vector3(25, 5, 5);
        private Vector3 newSize = new Vector3(1.5f, 1.5f, 1.5f);
        private float newX;

        public Transform snowballSize;
        public GameObject snowballPrefab;
        public GameObject snowball;
        public GameObject impactParticles;
        public AudioSource impactAudio;
        public AudioClip impactSound;
        public Vector3 cameraPosition, particlePos;
        public bool isAlive;
        public float speed;
        public bool hasSplit;

        void Start()
        {
            snowballs.Add(Instantiate(snowballPrefab, startingPos, Quaternion.identity));
            snowball = snowballs[snowballs.Count - 1];
            snowballSize = snowballs[snowballs.Count - 1].GetComponent<Transform>();
            snowballs[snowballs.Count - 1].GetComponent<Snowball>().player = this;
            isAlive = true;
            GameManager.Instance.isReloading = false;
            impactAudio = GetComponent<AudioSource>();
        }

        void FixedUpdate()
        {
            if (snowballs.Count > 0)
            {
                // Receiving vector3 for camera follow
                cameraPosition = cameraPos(snowballs[snowballs.Count - 1].transform.position);

                // Receiving vector3 for particle effect follow
                particlePos = ParticlePos(snowballs[snowballs.Count - 1].transform.position.z);

                // Snowball's speed
                speed = snowball.GetComponent<Snowball>().playerStats.speed;

                // checking for press of S key for split
                if (Input.GetKeyDown(KeyCode.S))
                {
                    // if snowball is large enough to split
                    if (snowballSize.localScale.x > resetSize.x)
                    { 
                        Split(snowballs[snowballs.Count - 1].transform.position);
                    }
                }
            }
        }

        void Split(Vector3 position)
        {
            
            if (position.x > 25)
            {
                newX = position.x - 3;
            }
            else if (position.x < 25)
            {
                newX = position.x + 3;
            }
            

            // iterate through list of snowballs
            for (int i = 0; i < snowballs.Count; i++)
            {
                // sets size of all snowballs in list to half
                snowballs[i].transform.localScale = snowballs[i].transform.localScale / 2;  
                
                // ensures snowball doesnt split smaller than starting size
                if (snowballs[i].transform.localScale.x < resetSize.x)
                {
                    snowballs[i].transform.localScale = newSize;
                }
            }

            GameObject snowball1 = Instantiate(snowball, new Vector3(newX, position.y + 3, position.z), Quaternion.identity);
            snowball1.transform.localScale = snowball.transform.localScale;
            snowballs.Add(snowball1);
        }

        Vector3 cameraPos(Vector3 position)
        {
            float newX = 0;
            float newY = position.y + 1.75f;
            float newZ = position.z - 4;
            float xPos;

            // If more than one snowball exists
            if (snowballs.Count > 1)
            {
                // iterate through positions of all snowballs in list
                for (int i = 0; i < snowballs.Count; i++)
                {
                    xPos = snowballs[i].transform.position.x;
                    newX += xPos;
                }

                // average out x-position of all snowballs and zoom out
                newX = newX / snowballs.Count;
                newZ = position.z - 6;
                Vector3 newCamPos = new Vector3(newX, newY, newZ); ;
                return newCamPos;
            }
            else
            {
                Vector3 singleCamPos = new Vector3(position.x, newY, newZ);
                return singleCamPos;
            }
        }

        Vector3 ParticlePos(float curZ)
        {
            Vector3 newParticlePos;
            newParticlePos.z = curZ + 225;
            newParticlePos = new Vector3(25, -25, newParticlePos.z);

            return newParticlePos;
        }

        //public void ResetSnowball()
        //{
        //    snowballs.Add(Instantiate(snowball, startingPos, Quaternion.identity));
        //    snowballSize = snowballs[snowballs.Count - 1].GetComponent<Transform>();
        //}

        public void RemoveSnowball(GameObject snowball1)
        {
            Vector3 pos = snowball1.transform.position;
            if (this.snowball == snowball1)
            {
                if (snowballs.Count > 1)
                {
                    this.snowball = snowballs[1];
                    snowballSize = snowballs[snowballs.Count - 1].GetComponent<Transform>();
                }
            }
            snowballs.Remove(snowball1);
            Destroy(snowball1);
            if (snowballs.Count <= 0)
            {
                isAlive = false;
                this.snowball = null;
            }

            PlayImpactSound();
            PlayImpactParticles(pos);
        }

        public void PlayImpactSound()
        {
            // play impact sound
            impactAudio.clip = impactSound;
            impactAudio.Play();
        }

        public void PlayImpactParticles(Vector3 snowball)
        {
            Instantiate(impactParticles, snowball, Quaternion.identity);
        }
    }
}

