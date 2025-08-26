using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VisualAttentionToolbox {
    public class FixationManager{


        private VisualAttentionDataController controller;

        //the fixationIdentifier algorithm used for the study (IVT ect)
        public FixationIdentifier fixationIdentifier;

        //list of all fixations that this manager deals with
        private List<Fixation> fixations;

        //Debug mode, displaying fixation centroids
        public GameObject debugFixationSphere;
       
        private int accessCount =  0;

        public List<Fixation> FixationList 
        {
            get { return fixations; } 
            private set { fixations = value; }
        }

        public FixationManager(FixationIdentifier newFixationIdentifier, VisualAttentionDataController passedController)
        {
            fixations = new List<Fixation>();

            fixationIdentifier = newFixationIdentifier;

            AssignDebugSpheres();

            AssignController(passedController);
        }
        public FixationManager(VisualAttentionDataController passedController)
        {
            fixations = new List<Fixation>();

            fixationIdentifier = new FixationIdentifier();

            AssignDebugSpheres();

            AssignController(passedController);
        }

        public void AssignController(VisualAttentionDataController passedController) 
        {
            //assign
            controller = passedController;
        }


        //THINK:: Alternative name for this function
        public void ConsiderNewGazePoint(IVTSpatial passedFixationIdentifier, GazePoint prevGazePoint, GazePoint newGazePoint)
        {
            //determine if the new gazePoint is still in the previous fixation
            bool isFixation  = passedFixationIdentifier.DetermineFixation(prevGazePoint, newGazePoint);

            //Encapsulate??

            if (newGazePoint == null) 
            {
                throw new System.Exception("newGazepoint passed as null into null FixationManager.ConsiderNewGazePoint for IVTSpatial Identifier");
            
            }


            //Determines what to do with the new fixation (if valid)
            IVTDecideFixationLogic(isFixation,newGazePoint,prevGazePoint);

        }


        public void ConsiderNewFixations(IVTSpatial passedFixationIdentifier, GazePoint prevLeftGazePoint, GazePoint newLeftGazePoint,
                                                                              GazePoint prevRightGazePoint, GazePoint newRightGazePoint)
        {

            //bool isLLFixation = passedFixationIdentifier.DetermineFixation

            //chain comparisons with the fixational logic???

            //Idea: consider if both are fixations in isolation, 

            //If LL fixation, then LR should add to a fixational

            //checks if the fixation identifier is of type IVTspatial
            if (fixationIdentifier is IVTSpatial)
            {
                //Debug.Log("3");
                //IVTDecideFixationLogic(isFixation, newGazePoint, prevGazePoint);

            }
            else
            {
                throw new System.Exception("the FixationManager does not support using this type of FixationIdentifier, consider extending ConsiderNewGazePoint ");

            }

        }

        //TODO:: REName
        private void AssignDebugSpheres()
        {
            debugFixationSphere = Resources.Load<GameObject>("VisualisationSpheres/FixationCentroidSphere");
        }

        public int GetFixationCount() 
        {
            return fixations.Count;
        }


        //WIP, need to take centroid after talk with michael
        public void ConsiderNewGazePoint(GazePoint prevLeftGazePoint , GazePoint prevRightGazePoint, GazePoint newLeftGazePoint, GazePoint newRightGazePoint, float leftIV, float rightIV)
        {
            
      
            //Needs serious cleanup
            GazePoint prevCentroid = null;
            if ((prevLeftGazePoint == null) && (prevRightGazePoint == null))
            {
                prevCentroid = null;
            } 
            else 
            {
                prevCentroid = new GazePoint();
                prevCentroid = prevCentroid.CreateCentroid(prevLeftGazePoint,prevRightGazePoint);
            }

            GazePoint newCentroid = null;
            if ((newLeftGazePoint == null) && (newRightGazePoint == null))
            {
                newCentroid = null;
            }
            else
            {
                newCentroid = new GazePoint();
                newCentroid = newCentroid.CreateCentroid(newLeftGazePoint, newRightGazePoint);
            }


            if ((prevCentroid != null) && (newCentroid != null))
            {
                //Debug.Log("Running correctly");

                //mean incoming velocity
                float avgIV = (leftIV + rightIV) / 2.0f;
                int returnCase = ConsiderNewGazePoint(prevCentroid, newCentroid, avgIV);

                //returnCase == 1:
                //propagate the new fixation id to all 4 fixations ID.


                if (returnCase == 1)
                {
                    int newFixationID = newCentroid.fixationID;

                    Fixation newFixation = null;

                    //TODO: This code is hard to read, consider refactoring into new methods, the logic may not be possible to simplify tho

                    //Assign the fixationID if not null
                    if (newLeftGazePoint != null)
                    {
                        newLeftGazePoint.fixationID = newFixationID;
                        if (newFixation != null)
                        {
                            newFixation.AddGazePointToFixation(newLeftGazePoint);
                        }
                        else 
                        {
                            newFixation = CreateAddNewFixationDebug(newLeftGazePoint);
                        
                        }
                    }

                    if (prevLeftGazePoint != null)
                    {
                        prevLeftGazePoint.fixationID = newFixationID;
                        //newFixation.AddGazePointToFixation(newLeftGazePoint);

                        if (newFixation != null)
                        {
                            newFixation.AddGazePointToFixation(prevLeftGazePoint);
                        }
                        else
                        {
                            newFixation = CreateAddNewFixationDebug(prevLeftGazePoint);

                        }

                    }

                    if (newRightGazePoint != null)
                    {
                        newRightGazePoint.fixationID = newFixationID;
                        //newFixation.AddGazePointToFixation(newLeftGazePoint);

                        //create fixation if needed, else add
                        if (newFixation != null)
                        {
                            newFixation.AddGazePointToFixation(newRightGazePoint);
                        }
                        else
                        {
                            newFixation = CreateAddNewFixationDebug(newRightGazePoint);

                        }


                    }

                    if (prevRightGazePoint != null)
                    {
                        prevRightGazePoint.fixationID = newFixationID;
                        //newFixation.AddGazePointToFixation(newLeftGazePoint);

                        if (newFixation != null)
                        {
                            newFixation.AddGazePointToFixation(prevRightGazePoint);
                        }
                        else
                        {
                            newFixation = CreateAddNewFixationDebug(prevRightGazePoint);

                        }
                    }

                    //update the centroid, fixation position, duration ect
                    newFixation.UpdateFixation();

                }
                else if (returnCase == 2)
                {
                    //case 2: add the new gazePoints to the already existing fixation.
                    int newFixationID = newCentroid.fixationID;

                    Fixation newFixation = fixations[newCentroid.fixationID];
                    //Assign the fixationID if not null

                    if (newLeftGazePoint != null)
                    {
                        newLeftGazePoint.fixationID = newFixationID;
                        newFixation.AddGazePointToFixation(newLeftGazePoint);
                    }

                    if (newRightGazePoint != null)
                    {
                        newRightGazePoint.fixationID = newFixationID;
                        newFixation.AddGazePointToFixation(newRightGazePoint);

                    }

                    //update the centroid, fixation position, duration ect
                    newFixation.UpdateFixation();

                } 
                else if (returnCase == 3) 
                {
                  //Saccadic point  
                
                }

            } 
            else
            {

            }

        }

        

        //Currently returns the fixation type (fixational saccadic ect)
        //1 is new value
        //2 is new 
        public int ConsiderNewGazePoint(GazePoint prevGazePoint, GazePoint newGazePoint, float incomingAngularVelocity) 
        {
            //Debug.Log("1");

            if (newGazePoint == null)
            {
                //Debug.Log("2");
                throw new System.Exception("newGazepoint passed as null into null FixationManager.ConsiderNewGazePoint for IVTSpatial Identifier");

            }

            //checks if the fixation identifier is of type IVTspatial
            if (fixationIdentifier is IVTSpatial)
            {
                //Debug.Log("3");
                bool isFixation = fixationIdentifier.DetermineFixation(prevGazePoint, newGazePoint);
                return BothEyeIVTDecideFixationLogic(isFixation, newGazePoint, prevGazePoint);

            }

            else if (fixationIdentifier is IVTAngular) 
            {
                //Debug.Log("running IVTAngular");
                bool isFixation = fixationIdentifier.DetermineFixation(prevGazePoint, newGazePoint,incomingAngularVelocity);
                return BothEyeIVTDecideFixationLogic(isFixation, newGazePoint, prevGazePoint);
            }
            else
            {
                throw new System.Exception("the FixationManager does support using this type of FixationIdentifier, consider extending ConsiderNewGazePoint ");

            }

            //not needed
            return -1;
        
        }

        private int GetFinalFixationIndex() 
        {
            int fixationListSize = fixations.Count - 1;
            if (fixationListSize < 1){ return 0; }
            
            else { return fixationListSize; }
        
        
        }

        private Fixation CreateAddNewFixationDebug(GazePoint newGazePoint) 
        {
            Fixation newFixation = new Fixation(newGazePoint);

            //add the two gazePoints to a 
            newFixation.UpdateFixation(newGazePoint);

            fixations.Add(newFixation);
            newFixation.ID = fixations.Count - 1; 

            //Debug.Log(("Size of fixationList after adding fixation is ", fixationList.Count) );

            //set the debugsphere
            newFixation.debugFixationSphere = this.debugFixationSphere;

            //This returns a reference to the created object, so need to set the fixation.debugsphere to this return
            var renderer = debugFixationSphere.GetComponent<Renderer>();

            renderer.enabled = controller.showDebugVisualisations;

            newFixation.debugFixationSphere = Object.Instantiate(this.debugFixationSphere, newFixation.CentroidPosition, Quaternion.identity);



            newFixation.UpdateDebugSphere();
            int fixationCount = fixations.Count;

            //Debug statement for adding first fixations
            if (fixationCount == 0)
            {
                Debug.Log("Adding the first fixation");
            }

            newGazePoint.fixationID = fixationCount - 1;

            return newFixation;

        }




        //Deprecated
        //Working on alternative
        public int ConsiderFrameGazePoints(GazePoint prevLeft, GazePoint newLeft, GazePoint prevRight, GazePoint newRight, float leftIV, float rightIV)
        {
            if (newLeft == null || newRight == null)
            {
                //Debug.Log("2");
                throw new System.Exception("newLeft, or newRight passed as null into FixationManager.ConsiderFrameGazePoints ");

            }

            bool leftIsFixation = false;
            bool rightIsFixation = false;

            //if the fixation is new, updated or not a fixation
            int fixationCase = -1;
            //checks if the fixation identifier is of type IVTspatial
            if (fixationIdentifier is IVTSpatial)
            {
                leftIsFixation = fixationIdentifier.DetermineFixation(prevLeft, newLeft);
                rightIsFixation = fixationIdentifier.DetermineFixation(prevRight, newRight);
            }

            else if (fixationIdentifier is IVTAngular)
            {
                leftIsFixation = fixationIdentifier.DetermineFixation(prevLeft, newLeft, leftIV);
                rightIsFixation = fixationIdentifier.DetermineFixation(prevRight, newRight, rightIV);

                if (leftIsFixation && rightIsFixation)
                {



                }


            }
            else
            {
                throw new System.Exception("the fixationManager does not support this type of FixationIdentifier, consider extending ConsiderNewGazePoint");
            }

            return fixationCase;
        }



        private void IVTDecideFixationLogic(bool isFixation, GazePoint newGazePoint, GazePoint prevGazePoint)
        {
            //if the two points are a fixation,
            if (isFixation)
            {

                //Debug.Log(("PREV Gaze point hashcode is ", prevGazePoint.GetHashCode()));
                //Debug.Log(("NEW Gaze point hashcode is ", newGazePoint.GetHashCode()));

                //Debug.Log(("Previous fixationID = ", prevGazePoint.fixationID));


                if (prevGazePoint.fixationID != -1) 
                {
                    Debug.Log(("Found a gazepoint already associated with a FIXATION\n, fixationID is ", prevGazePoint.fixationID));
                
                
                }
                //Debug.Log(("access Count is ", accessCount));

                accessCount++;

                //previous gaze point is not currently in a fixation.
                //so create a new fixation
                if (prevGazePoint.fixationID == -1)
                {
                    Debug.Log("CASE 1\nCreating new fixation with the new and previous gazePoints");
                    Fixation newFixation = CreateAddNewFixationDebug(prevGazePoint);

                    //Debug.Log(("prevGazePoint fixationID set to = ", prevGazePoint.fixationID));
                    newFixation.UpdateFixation(newGazePoint);
                    newFixation.UpdateDebugSphere();

                    newGazePoint.fixationID = fixations.Count - 1;
                    
                    //Debug.Log(("NewGazePoint givenFixationID of ", newGazePoint.fixationID));

                }
                else 
                {
                    Debug.Log("CASE 2");
                    Debug.Log("Updating previous fixational point");
                    //case where the two gazePoints are part of a fixation, and the previous gaze point is part of a fixation 
                    //Add the new fixation to the previous one.
                    int fixationCount = fixations.Count;

                    if (fixationCount == 0)
                    {
                        Debug.Log(("Previous gaze point has fixationID = ", prevGazePoint.fixationID));
                        //Need to guard against null pointer error
                        throw new System.Exception("Previous gazePoint has fixation, but that fixation has not been logged in the fixationManager");
                        //CreateAddNewFixationDebug(prevGazePoint);
                    }

                    Fixation lastFixation = fixations[fixationCount - 1];

                    

                    //only add this fixation
                    lastFixation.UpdateFixation(newGazePoint);

                    //make sure the new gaze point is assigned the same fixationID as the previous one
                    //they are part of the same fixation
                    newGazePoint.fixationID = prevGazePoint.fixationID;

                    //Debug only

                    //Debug.Log(("DebugFixationSphere position before update = ", lastFixation.debugFixationSphere.transform.position));


                    lastFixation.UpdateDebugSphere();

                    //Debug.Log(("DebugFixationSphere position AFTER update = ", lastFixation.debugFixationSphere.transform.position));

                    //var renderer = lastFixation.debugFixationSphere.gameObject.GetComponent<Renderer>();

                    //renderer.sharedMaterial.SetColor("_Color", Color.blue);

                }

            }
            else
            {
                //Debug.Log(("access Count is ", accessCount));

                accessCount++;
                //two gazePoints are not part of a fixation,
                // EDGE CASE::nothing was collided last frame, but we still need to determine if this point is a fixation or not

                //Options:
                //1. Assume the first point after hitting nothing is a fixation 
                //2. Assume it is not a fixation
                //3. Decide on the next frame, if t
                //  - if the next point and the previous point are within IVT then create a new fixation, else dont

                //Option 1 if the previous gazePoint is null (no gaze hit) then (ASSUME) that this point is a new fixation 
                //This needs further consideration
                //TODO: Consider it further

                //CreateAddNewFixationDebug(newGazePoint);

                //Debug.Log("(DISABLED) Creating new fixation point (two points are not in same fixation)");

            }

            //Debug.Log(("size of fixationList in fixationManager is",fixationList.Count));


        }


        /// <summary>
        /// Logic for handling fixations using IVT for BOTH eye, does not create fixations, just returns the case
        /// </summary>
        /// <param name="isFixation"></param>
        /// <param name="newGazePoint"></param>
        /// <param name="prevGazePoint"></param>
        /// <returns>
        /// 1 if new fixation created that contains prev and new gazePoints
        /// 2. if new fixation created, with just the new gazePoints
        /// 3. If no new fixation created.
        /// 
        /// </returns>
        private int BothEyeIVTDecideFixationLogic(bool isFixation, GazePoint newGazePoint, GazePoint prevGazePoint)
        {
            //if the two points are a fixation,
            if (isFixation)
            {
                //previous gaze point is not currently in a fixation.
                //so create a new fixation
                if (prevGazePoint.fixationID == -1)
                {

                    newGazePoint.fixationID = fixations.Count - 1;

                    //case 1: a new fixation (with new fixationID's needs to be propagated to the 4 gazePoints
                    return 1;

                }
                else
                {
                    //case where the two gazePoints are part of a fixation, and the previous gaze point is part of a fixation 
                    //Add the new fixation to the previous one.
                    int fixationCount = fixations.Count;

                    if (fixationCount == 0)
                    {
                        //Need to guard against null pointer error
                        throw new System.Exception("Previous gazePoint has fixation, but that fixation has not been logged in the fixationManager");
                        //CreateAddNewFixationDebug(prevGazePoint);
                    }

                    Fixation lastFixation = fixations[fixationCount - 1];


                    //make sure the new gaze point is assigned the same fixationID as the previous one
                    //they are part of the same fixation
                    //Give both the same fixationID
                    newGazePoint.fixationID = prevGazePoint.fixationID;

                    //CASE 2: the previous and new points are part of a fixation, but the previous points already are fixational points.
                    //The previous points fixation must be propagated to the other gazePoints in the fixations.
                    return 2;

                }

            }
            else
            {
                accessCount++;
                //two gazePoints are not part of a fixation,
                // EDGE CASE::nothing was collided last frame, but we still need to determine if this point is a fixation or not

                //Options:
                //1. Assume the first point after hitting nothing is a fixation 
                //2. Assume it is not a fixation (USED)
                //3. Decide on the next frame, 
                //  - if the next point and the previous point are within IVT then create a new fixation, else dont

                //Debug.Log("(DISABLED) Creating new fixation point (two points are not in same fixation)");
                //case 3: This isnta  fixation, so no changes have to be made to the working
                return 3;
            }

        }


        // <summary>
        /// Set fixation Visualisations based on a flag
        /// </summary>
        public void ShowFixationVisualisations(bool visualiseFixations)
        {
            foreach (Fixation f in fixations) 
            {
                var renderer = f.debugFixationSphere.GetComponent<Renderer>();
                renderer.enabled = visualiseFixations;

            }
        }

    }

}
