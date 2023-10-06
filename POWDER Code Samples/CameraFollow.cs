using Powder.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 camDistance;

    void Update()
    {
        camDistance = GameManager.Instance.player.cameraPosition;
        transform.position = camDistance;
    }
}
