using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// test class for the DOP, moves an object in a loop through waypoints specified by object positions in editor 
/// //This code roughly follows a tutorial https://www.youtube.com/watch?v=oaFJBP4Ld7k, and was just used for demonstration purposes.
/// </summary>
public class DOPMovementScript : MonoBehaviour
{

    [SerializeField]
    public List<GameObject> demoWayPoints;

    [SerializeField]
    public float moveSpeed;

    [SerializeField]
    private int wayPointIndex;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (wayPointIndex >= demoWayPoints.Count)
        {
            //Debug.Log("Why isnt it looping");
            wayPointIndex = 0;
        }

        Vector3 destPosition = demoWayPoints[wayPointIndex].transform.position;

        //new position is the old position -> move position moving along at the move speed
        Vector3 newPosition = Vector3.MoveTowards(transform.position, destPosition, moveSpeed);
        
        //update the position (of this object)
        transform.position = newPosition;

        float distance = Vector3.Distance(transform.position, destPosition);
        
        //handle near misses (move to next waypoint if overshot/undershot the next one)
        if (distance <= 0.05) 
        {
            wayPointIndex++;
        }
        else 
        {   
            //loop forever
            
        }


        
    }
}
