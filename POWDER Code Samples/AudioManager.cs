using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Powder.Singleton
{
    public class AudioManager : Singleton<AudioManager>
    {
        public AudioSource backgroundMusic;
        public AudioClip[] music;

        private void Start()
        {
            playRandomMusic();
        }

        private void Update()
        {
            // checking if music is playing
            if (!backgroundMusic.isPlaying)
            {
                playRandomMusic();
            }
        }

        void playRandomMusic()
        {
            backgroundMusic.clip = music[Random.Range(0, music.Length)] as AudioClip;
            backgroundMusic.Play();
        }
    }
}

