using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace VisualAttentionToolbox {
    public class AreaOfInterestScript : MonoBehaviour
    {

        //list of all gazePoints that are associated with this object (lie on the object)
        private List<GazePoint> objectGazePoints;

        public float firstFoundTime = -1;
        public int firstFoundFrameCount = -1;

        public int numberOfGazes = 0;

        public float avgGazeDuration = 0;

        public List<GazePoint> ObjectGazePoints 
        {
            get { return objectGazePoints;  }
            set { objectGazePoints = value; } 
        }

        private float gazeDuration;

        public float GazeDuration
        {
            private set { gazeDuration = value; }

            get { return gazeDuration;  }
        
        }


        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("started up AreaofInterestScript");
            objectGazePoints = new List<GazePoint>();
        }

        /// <summary>
        /// work done in here should be minimised, as multiple AOIs possible,
        /// currently just updates the gazeDuration for time of gaze on AOI objects
        /// </summary>
        private void Update()
        {

            UpdateCumulativeGazeDuration();

        }


        void UpdateCumulativeGazeDuration() 
        {
            int currentFrameCount = Time.frameCount;

            GazePoint lastInChain = consecutiveChains.LastOrDefault();
            if (lastInChain == null)
            {
                //No chain, so start a new one
            }
            else 
            {
                //find the first and last in the consecutive chain
                //

                if (consecutiveChains.Count >= 2) 
                {
                    //Debug.Log("Valid chains");


                    //GazePoint secondLastInChain = consecutiveChains[consecutiveChains.Count-2];

                    GazePoint secondLastInChain = FindSecondLastInChain();

                    float secondToLastGazeTime = secondLastInChain.gazeTime;
                    //Debug.Log(("Second To last time = ",secondToLastGazeTime));
                    
                    float endGazeTime =  lastInChain.gazeTime;
                    //Debug.Log(("end time = ", endGazeTime));

                    if ((lastInChain.frameCount + 1) == currentFrameCount) 
                    {
                        cumulativeGazeDuration += Mathf.Abs(endGazeTime - secondToLastGazeTime);

                        if (numberOfGazes > 0) 
                        {
                            avgGazeDuration = cumulativeGazeDuration / numberOfGazes;
                        }
                        //print(("Updated cumGazeDur= ", cumulativeGazeDuration));
                        
                    }

                }

            }

        }


        private GazePoint FindSecondLastInChain() 
        {
            GazePoint finalInChain = consecutiveChains.LastOrDefault();

            int currFrameCount = finalInChain.frameCount;


            //go backwards and find the first gazePoint that is on the previosu chain
            for (int i = consecutiveChains.Count - 2; i >= 0; i--) 
            {
                GazePoint gp = consecutiveChains[i];
                if (currFrameCount != gp.frameCount) 
                {
                    //second to last in chain as first element with different frame count
                    return gp;
                }
                       
            }


            return finalInChain;
        
        }

        /// <summary>
        /// Appends a gazePoint (that hit this Object) onto this objects list of gazePoints
        /// Will be used to move gazePoints along with the rotation and translation of the other gazePoints
        /// </summary>  
        /// <param name="newGazePoint"></param>
        /// 


        //List of (current) chain of consecutive gaze points, this is important for finding the 
        private List<GazePoint> consecutiveChains = new List<GazePoint>();

        private float cumulativeGazeDuration = 0.0f;

        public float CumulativeGazeDuration
        {
            get { return cumulativeGazeDuration; }

            private set { cumulativeGazeDuration = value; }
        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGazePoint"></param>
        public void AddGazePoint(GazePoint newGazePoint) 
        {
            //Store the first time the object was found (helpful for visual search)
            if (firstFoundTime == -1) 
            {
                firstFoundTime = Time.time;
                firstFoundFrameCount = Time.frameCount;
            }


            //adds a new gazePoint to this
            objectGazePoints.Add(newGazePoint);

            //update the chain
            //print(("Before cumGazeDur", cumulativeGazeDuration));
            UpdateCumulativeGazeDuration(newGazePoint);
            //print(("After cumGazeDuration", cumulativeGazeDuration));




        }


        /// <summary>
        /// Updates the gazeTime, by finding the sum of consecutive subchain (unbroken gaze points)
        /// </summary>
        /// <param name="newGazePoint"></param>
        private void UpdateCumulativeGazeDuration(GazePoint newGazePoint) 
        {
            //Update the chainsize
            int gazeChainSize = consecutiveChains.Count;

            //get the last 
            GazePoint lastInChain = consecutiveChains.LastOrDefault();
            if (lastInChain == null)
            {
                //No chain, so start a new one
                consecutiveChains.Add(newGazePoint);
                numberOfGazes++;
                //Debug.Log("Case1, no gazePoints in chain, so starting one");
            }
            else 
            {
                //check if the new gazePoint is on the same or the next frame

                if ((newGazePoint.frameCount == lastInChain.frameCount) || (newGazePoint.frameCount == (lastInChain.frameCount + 1)))
                {
                    //The gazePoint is still in the same chain, so add to it
                    //Debug.Log("Case2;, gaze points are part of the same chain");
                    consecutiveChains.Add(newGazePoint);
                }
                else 
                {
                    //Debug.Log("Case3; gaze points are NOT part of the same chain");
                    //the newGazePoint is not in the same chain, so need to find the start and end
                    numberOfGazes++;

                    GazePoint firstInChain = consecutiveChains[0];

                    float startGazeTime = firstInChain.gazeTime;
                    //Debug.Log(("StartTime is ", startGazeTime));

                    float endGazeTime = lastInChain.gazeTime;
                    //Debug.Log(("endGazeTime is ", endGazeTime));

                    //start a new chain
                    consecutiveChains = new List<GazePoint>();

                    consecutiveChains.Add(newGazePoint);
                    Debug.Log("Created a new chain!!");
                    
                }
            
            }

        }


    }
}
