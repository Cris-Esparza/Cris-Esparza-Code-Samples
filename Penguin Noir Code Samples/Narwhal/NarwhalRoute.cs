using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarwhalRoute : MonoBehaviour
{
    [SerializeField]
    private Transform[] points;     //The points for the movement
    public Transform[] Points { get { return points; } }    //Get for the points
    private Vector2 gizmosPosition; //Store gizmo vector 2


    /// <summary>
    /// Draws the line for the narwhal path
    /// </summary>
    private void OnDrawGizmos()
    {
        //for each point, draws the position
        for (float t = 0; t <= 1; t += 0.05f)
        {
            gizmosPosition = Mathf.Pow(1 - t, 3) * points[0].position + 3 * 
                             Mathf.Pow(1 - t, 2) * t * points[1].position + 3 * (1 - t) * 
                             Mathf.Pow(t, 2) * points[2].position + 
                             Mathf.Pow(t, 3) * points[3].position;

            Gizmos.DrawSphere(gizmosPosition, 0.25f);
        }

        //Draw the lines
        Gizmos.DrawLine(new Vector2(points[0].position.x, points[0].position.y), new Vector2(points[1].position.x, points[1].position.y));
        Gizmos.DrawLine(new Vector2(points[2].position.x, points[2].position.y), new Vector2(points[3].position.x, points[3].position.y));
    }
}
