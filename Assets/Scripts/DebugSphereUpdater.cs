using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualAttentionToolbox
{


    /// <summary>
    /// Depreacate: Class is not used
    /// </summary>
    public class DebugSphereUpdater : MonoBehaviour
    {
        GazePoint myGazePoint;
        
        // Start is called before the first frame update
        void Start()
        {
            //basically do nothing
            myGazePoint = null;
        }

        // Update is called once per frame
        //https://stackoverflow.com/questions/64670134/how-to-rotate-around-an-object-without-using-unitys-built-in-functions (considere this)
        void Update()
        {

            //Vector3 currLocalPosition = transform.localPosition;

            Vector3 newLocalPosition = myGazePoint.ObjectSpacePosition;




            transform.position = newLocalPosition;











        }

        void assignGazePoint(GazePoint gp) 
        {
            myGazePoint = gp;
        
        }
    }
}
