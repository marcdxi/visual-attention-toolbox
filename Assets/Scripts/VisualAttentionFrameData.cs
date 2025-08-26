using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VisualAttentionToolbox
{

    /// <summary>
    /// Contains all the useful information for logging to csv files for each FRAME 
    /// of running,
    ///
    /// </summary>
    public struct VisualAttentionFrameData    {

        public readonly int frameCount;

        //can be null, in that case set the positions to be null
        public readonly GazePoint leftEyeGazePoint;
        public readonly GazePoint rightEyeGazePoint;

        //Confidence rate for eyes (0->1)
        public readonly float leftEyeOpen;
        public readonly float rightEyeOpen;

        //positions

        public readonly Vector3 leftEyePosition;
        public readonly Vector3 rightEyePosition;
        public readonly Vector3 headCameraPosition;


        //Fixational or Saccadic eye movement.
        public readonly bool isFixational;

        //Conjugate or disconjugate (eyes looking at the same or different objects)
        public readonly bool isConjugate;

        public readonly float incomingLeftEyeAngularVelocity;

        public readonly float incomingRightEyeAngularVelocity;

        public readonly float incomingHeadAngularVelocity;

        /// <summary>
        /// Only way to access the values within struct, input all values, even if they are null.
        /// This information is important for the raw, frame by frame output of ALL data for csv files
        /// </summary>
        /// <param name="passedLeftEyeGazePoint"></param>
        /// <param name="passedRightEyeGazePoint"></param>
        /// <param name="leftEyeOpenConfidence"></param>
        /// <param name="rightEyeOpenConfidence"></param>
        /// <param name="passedHeadCameraPosition"></param>
        /// <param name="passedLeftEyePos"></param>
        /// <param name="passedRightEyePos"></param>
        /// <param name="passedFrameCount"></param>
        /// <param name="passedIsFixation"></param>
        /// <param name="passedIsConjugate"></param>
        /// <param name="passedIncomingLeftEyeAngularVelocity"></param>
        /// <param name="passedIncomingRightEyeAngularVelocity"></param>
        /// <param name="passedIncomingHeadAngularVelocity"></param>
        public VisualAttentionFrameData(GazePoint passedLeftEyeGazePoint, GazePoint passedRightEyeGazePoint, float leftEyeOpenConfidence, 
                                        float rightEyeOpenConfidence, Vector3 passedHeadCameraPosition, Vector3 passedLeftEyePos, 
                                        Vector3 passedRightEyePos, int passedFrameCount, bool passedIsFixation, bool passedIsConjugate, 
                                        float passedIncomingLeftEyeAngularVelocity, float passedIncomingRightEyeAngularVelocity, 
                                        float passedIncomingHeadAngularVelocity) 
        {

            leftEyeGazePoint = passedLeftEyeGazePoint;
            rightEyeGazePoint = passedRightEyeGazePoint;

            leftEyeOpen = leftEyeOpenConfidence;
            rightEyeOpen = rightEyeOpenConfidence;

            headCameraPosition = passedHeadCameraPosition;
            leftEyePosition = passedLeftEyePos;
            rightEyePosition = passedRightEyePos;

            frameCount = passedFrameCount;

            isFixational = passedIsFixation;
            isConjugate = passedIsConjugate;

            incomingLeftEyeAngularVelocity = passedIncomingLeftEyeAngularVelocity;
            incomingRightEyeAngularVelocity = passedIncomingRightEyeAngularVelocity;
            incomingHeadAngularVelocity = passedIncomingHeadAngularVelocity;

        }

    }
}
