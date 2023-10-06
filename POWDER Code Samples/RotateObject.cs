using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public int xRotateSpeed;
    public int yRotateSpeed;
    public int zRotateSpeed;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(xRotateSpeed, yRotateSpeed, zRotateSpeed, Space.World);
    }
}
