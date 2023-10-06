using Powder.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleMovement : MonoBehaviour
{
    Vector3 particlePos;

    void Update()
    {
        particlePos = GameManager.Instance.player.particlePos;
        transform.position = particlePos;
    }
}
