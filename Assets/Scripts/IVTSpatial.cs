using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//Velocity threshold Identification (I-VT) using Spatial velocities (Units/s)

namespace VisualAttentionToolbox
{
    public class IVTSpatial : FixationIdentifier
    {
        //upper spatial velocity threshold
        public readonly float spatialVelocityThreshold;

        public IVTSpatial(float upperSpatialVT) 
        {
            
            //set upper spatial velocity 
            spatialVelocityThreshold = upperSpatialVT;

        }

        //Determines if two gazePoints are fixations
        public override bool DetermineFixation(GazePoint prevGP, GazePoint nextGP) {

            
            //handle null
            if (prevGP == null || nextGP == null) 
            {
                return false;    
            }


            if (useWorldPositions)
            {
                return CheckIVTFixationWorld(prevGP, nextGP);

            }
            else 
            {
                return CheckIVTFixationLocal(prevGP, nextGP);            
            }
            
        }

        //REPEATED CODE, POSSIBLY REFACTOR
        public bool CheckIVTFixationWorld(GazePoint gp0, GazePoint gp1)
        {

            //find the absolute distance 
            float absDistance = findDistance(gp0.WorldSpacePosition, gp1.WorldSpacePosition);

            //Debug.Log(("Abs difference seen = ",absDistance) );

            float absTimeDifference = Mathf.Abs(gp1.gazeTime - gp0.gazeTime);


            //Debug.Log(("Abs TimeDifference seen = ", absTimeDifference));

            float absVelocity = absDistance / absTimeDifference;

            //Debug.Log(("Percieved Spatial Velocity = ", absVelocity));

            //As per IVT algorithm, if velocity below (or equal to) velocity threshold then gazepoints are part of the same fixation

            //returns true if below/equal threshold, false if above
            return (absVelocity <= spatialVelocityThreshold);




        }

        //REPEATED CODE, POSSIBLY REFACTOR
        //check if two gazePoints are in a fixation using localSpace
        public bool CheckIVTFixationLocal(GazePoint gp0, GazePoint gp1)
        {

            //get the absolute distance betwern the two gazePoints supplied in objectSpace
            float absDistance = findDistance(gp0.ObjectSpacePosition,gp1.ObjectSpacePosition);

            float absTimeDifference = Mathf.Abs(gp1.gazeTime - gp0.gazeTime);

            float absVelocity = absDistance / absTimeDifference;
            
            //As per IVT algorithm, if velocity below (or equal to) velocity threshold then gazepoints are part of the same fixation

            return (absVelocity <= spatialVelocityThreshold);


        }


        private float findDistance(Vector3 start, Vector3 end) 
        {
            Vector3 diff = end - start;

            return diff.magnitude;
        }

        
    }
}
