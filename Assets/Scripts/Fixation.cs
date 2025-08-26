using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VisualAttentionToolbox
{

    
    /// <summary>
    /// Class that stores all information about Fixations, including 
    /// Centroid position
    /// durations, 
    /// size (num gaze points), 
    /// the debug visualisation, 
    /// references to related gaze points
    /// fixationID
    /// </summary>

    public class Fixation
    {

        //Debug mode, displaying fixation centroids 
        public GameObject debugFixationSphere;

        //list of all GazePoints in the fixation
        public List<GazePoint> gazePointList;

        //for use later, potentially use a hasmap
        public float ID;

        //Position of centroid for fixation in space;
        private Vector3 centroidPosition;

        public Vector3 CentroidPosition
        {
            private set { centroidPosition = value; }
            get { return centroidPosition; }

        }

        private Vector3 baseScale;

        //Position of centroid for fixation in TIME;
        private float centroidTime;

        public float CentroidTime
        {
            private set { centroidTime = value; }
            get { return centroidTime; }
        }

        private float duration;
        public float Duration
        {
            private set { duration = value; }
            get { return duration; }
        }

        public Fixation()
        {
            gazePointList = new List<GazePoint>();
            AssignDebugSpheres();
        }

        /// <summary>
        /// loads the debug sphere prefab from files, and stores its original intended scale (scaled up as fixation length increaes)
        /// </summary>
        private void AssignDebugSpheres()
        {
            debugFixationSphere = Resources.Load<GameObject>("VisualisationSpheres/FixationCentroidSphere");

            //store the original scale
            baseScale = debugFixationSphere.gameObject.transform.localScale;
        }


        /// <summary>
        /// Creates a new fixation with the gazePoint specified, calcualtes centroids
        /// </summary>
        public Fixation(GazePoint newGazePoint) 
        {
            gazePointList = new List<GazePoint>();

            UpdateFixation(newGazePoint);

            AssignDebugSpheres();

        }

        /// <summary>
        /// Updates the visuaisations, centroids and fixation durration when a new gazePoint is added.
        /// </summary>
        /// <param name="newGazePoint"></param>
        public void UpdateFixation(GazePoint newGazePoint) 
        {
            AddGazePointToFixation(newGazePoint);
            UpdateCentroids();

            UpdateDuration();

        }

        /// <summary>
        /// manually re-calculates fixaiton information
        /// </summary>
        public void UpdateFixation()
        {
            UpdateCentroids();

            UpdateDuration();

            UpdateDebugSphere();
        }

        /// <summary>
        /// Updates the duration of a fixation, 
        /// Takes time diff betwern first gazePoint and lastGazePoint 
        /// </summary>
        private void UpdateDuration() 
        {
            float firstTime = gazePointList[0].gazeTime;
            float lastTime = gazePointList[gazePointList.Count - 1].gazeTime;

            float timeDifference = lastTime - firstTime;
            duration = timeDifference;

            //update the scale of the fixation
            

        }


        public void AddGazePointToFixation(GazePoint newGazePoint) 
        {   
            gazePointList.Add(newGazePoint);
        }



        

        /// <summary>
        /// Recalculates the centroidPosition and centroidTime variables in worldspace
        /// </summary>
        public void UpdateCentroids() 
        {

            //centroid is mean position and time
            float sumTime = 0.0f;

            //Vector3 sumPosition = new Vector3();

            float sumXPos = 0.0f;

            float sumYPos = 0.0f;

            float sumZPos = 0.0f;

            foreach (GazePoint gp in gazePointList) 
            {
                sumTime += gp.gazeTime;

                //sumPosition += gp.WorldSpacePosition;

                sumXPos += gp.WorldSpacePosition.x;

                sumYPos += gp.WorldSpacePosition.y;

                sumZPos += gp.WorldSpacePosition.z;

            }


            //mean position in worldspace
            centroidPosition.x = sumXPos / gazePointList.Count;
            centroidPosition.y = sumYPos / gazePointList.Count;
            centroidPosition.z = sumZPos / gazePointList.Count;

            //mean time of gazepoints
            centroidTime = sumTime / gazePointList.Count;

        }


        /// <summary>
        /// updates the fixation visualisation, setting its position correctly, and scaling the visualisation
        /// </summary>
        public void UpdateDebugSphere() 
        {
           // Debug.Log(("new debugFixationSphere position should be", centroidPosition));
            debugFixationSphere.transform.position = centroidPosition;

            //scale the fixation sphere based on duration
            debugFixationSphere.gameObject.transform.localScale = (baseScale * duration *0.5f) + baseScale;

        }

    }
}
