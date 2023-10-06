using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmingTube : MonoBehaviour
{
    [Header("To Be Non-serializable")]
    public Vector2 tubeDirection;
    //Creates public vectors of the tube sides to be accessed by the penguin
    public Vector2 tubeSideA;
    public Vector2 tubeSideB;
    public void Start()
    {
        //Sets tube side vectors to the empty game objects located within the tube prefab
        tubeSideA = transform.GetChild(0).transform.position;
        tubeSideB = transform.GetChild(1).transform.position;
        tubeDirection = gameObject.transform.right;
    }

}
