using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualAttentionToolbox
{

    //Velocity threshold Identification (I-VT) using angular velocities (Deg/s)
    public class IVTAngular : FixationIdentifier
    {
        //upper angular velocity threshold
        public readonly float angularVelocityThreshold;
        
        
        public IVTAngular(float upperAngularVT) {

            //set upper angular velocity 
            angularVelocityThreshold = upperAngularVT;

        }

        /// <summary>
        /// Determines if two gazePoints are part of the fixation along IVT (fixational points if below a IVT threshold)
        /// </summary>
        /// <param name="prev"> previous GazePoint, Do not just make up a gaze point for this function </param>
        /// <param name="next"> new GazePoint (this frame), Do not just make up a gaze point for this function  </param>
        /// <param name="incomingVelocity"> the incoming anglular velocity prev->new in degrees/s </param>
        /// <returns></returns>
        public override bool DetermineFixation(GazePoint prev, GazePoint next, float incomingAngularVelocity)
        {
            //handle null inputs, This function should return null if valid gazePoints are not given,
            //a fixational point CANNOT exist without a gazePoint (intersection of eye ray in environment)
            if ((prev == null) || (next == null)) 
            {
                Debug.Log("NULL gazePoints passed into determineFixation angular");    
                return false; 
            }

            //Debug.Log(("Incoming angular velocity of ", incomingAngularVelocity, "was determined", (incomingAngularVelocity <= angularVelocityThreshold) ));


            //true if below angularVelocityThreshold, false if above
            return (incomingAngularVelocity <= angularVelocityThreshold);
        }



    }
}
