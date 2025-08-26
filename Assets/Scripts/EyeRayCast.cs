using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VisualAttentionToolbox
{
    public class EyeRayCast : MonoBehaviour
    {
        [SerializeField]
        private GameObject HeadAnchor;

        [SerializeField]
        //determine if the eye raycaster should cast rays (defaults to false)
        public bool castEyeRays = false;

        [SerializeField]
        //layers for use in the raycast (by defaulteverything will be seen by this, but should be easy to extend so that UI elements dont get hit by the raycast
        //(maybe cast two rays) one for UI elements, and one for the environment? though could be done by just returning all things hit by the ray and sorting from there?
        public LayerMask layersToCollide;

        //Some functionality (like eyes open/closed can be used from this (refactor
        public OVREyeGaze eyeGaze;

        //only way to find if eyes open/closed
        public OVRFaceExpressions faceExpressions;

        [SerializeField]
        //left or right eye
        private bool isLeft;

        //holds the position vector for the object (eye)
        private Vector3 eyePos;
        [SerializeField]
        private float rayLength = 1.0f;
        //FOR DEV, Remove later
        public GameObject gazePointSphere;

        //For DEV REMOVE LATER
        public GameObject cameraPointSphere;

        [SerializeField]
        public GazeMap gazeMap;

        //For debug, later on this needs to be abstracted and some functionality moved
        //Thresholding for IVT, at the moment
        [SerializeField]
        public float IVTThreshold;

        private GazePoint previousGazePoint = null;


        [SerializeField]
        public GameObject fixationDebugSphere;

        //Modifiable keycode for togglingRayCasts (default space)
        [SerializeField]
        public KeyCode toggleRayCastKeyCode = KeyCode.Space;

        //Modifiable keycode for toggling heatMap Modification (default L) 
        [SerializeField]
        public KeyCode heatMapModifyKeyCode = KeyCode.L;

        private bool overlayHeatmap = false;

        private List<List<RaycastHit>> sortedHitsByLayer;
        //List of hits by layer this frame, is set to null at the start of each update()
        public List<List<RaycastHit>> SortedHitsByLayer
        {
            get { return sortedHitsByLayer; }
            private set { sortedHitsByLayer = value; }
        }

        private List<List<RaycastHit>> prevSortedHitsByLayer;
        public List<List<RaycastHit>> PrevSortedHitsByLayer
        {
            get { return prevSortedHitsByLayer; }
            private set { prevSortedHitsByLayer = value; }
        }

        //Need to expose this value to VisualAttentionDataController, in addition to the list of sorted RaycastHits

        private List<List<GazePoint>> prevGazePointsByLayer;
        public List<List<GazePoint>> PrevGazePointsByLayer
        {
            get { return prevGazePointsByLayer; }
            private set { prevGazePointsByLayer = value; }
        }


        private List<List<GazePoint>> gazePointsByLayer;
        public List<List<GazePoint>> GazePointsByLayer
        {
            get { return gazePointsByLayer; }
            private set { gazePointsByLayer = value; }
        }

        private Quaternion prevHeadRotation;

        public Quaternion PrevHeadRotation
        {
            get { return prevHeadRotation; }
            private set { prevHeadRotation = value; }
        }

        private Quaternion newHeadRotation;

        public Quaternion NewHeadRotation
        {
            get { return newHeadRotation; }
            private set { newHeadRotation = value; }
        }

        private Quaternion prevEyeRotation;

        public Quaternion PrevEyeRotation
        {
            get { return prevEyeRotation; }
            private set { prevEyeRotation = value; }
        }

        private Quaternion newEyeRotation;

        public Quaternion NewEyeRotation
        {
            get { return newEyeRotation; }
            private set { newEyeRotation = value; }
        }


        private float eyeClosedConfidence;

        public float EyeClosedConfidence
        {
            get { return eyeClosedConfidence; }
            private set { eyeClosedConfidence = value; }
        }


        //Array of expressions for this frame, could be used to 
        private float[] frameExpressions;
        public float[] FrameExpressions 
        {
            get { return frameExpressions; }
            private set { frameExpressions = value; }
        }

        private bool expressionsAreValid = false;

        public bool ExpressionsAreValid
        {
            get { return expressionsAreValid; }
            private set { expressionsAreValid = value; }

        }


        private float upperFaceConfidence = -1;

        public float UpperFaceConfidence
        {
            get { return upperFaceConfidence; }
            private set { upperFaceConfidence = value; }
        }

        private float lowerFaceConfidence = -1;

        public float LowerFaceConfidence
        {
            get { return lowerFaceConfidence; }
            private set { lowerFaceConfidence = value; }
        }



        //Time of previous frame, not duration
        private float prevFrameTime = -1.0f;
        public float PrevFrameTime
        {
            get { return prevFrameTime; }
            private set { prevFrameTime = value; }
        }

        //time of new (current) frame, not duration
        private float newFrameTime = -1.0f;
        public float NewFrameTime
        {
            get { return newFrameTime; }
            private set { newFrameTime = value; }
        }

        private float newEyeAngularVelocity = -1.0f;

        public float NewEyeAngularVelocity
        {
            get { return newEyeAngularVelocity; }
            private set { newEyeAngularVelocity = value; }
        }

        private float prevEyeAngularVelocity = -1.0f;

        public float PrevEyeAngularVelocity
        {
            get { return prevEyeAngularVelocity; }
            private set { prevEyeAngularVelocity = value; }
        }

        private float newHeadAngularVelocity = -1.0f;

        public float NewHeadAngularVelocity
        {
            get { return newHeadAngularVelocity; }
            private set { newHeadAngularVelocity = value; }
        }

        private float prevHeadAngularVelocity = -1.0f;

        public float PrevHeadAngularVelocity
        {
            get { return prevHeadAngularVelocity; }
            private set { prevHeadAngularVelocity = value; }
        }
        //End of getters and setters


        /// <summary>
        /// Called on program start, when this object is brought into scene.
        /// Sets up the datastructures necessary.
        /// </summary>
        private void Awake()
        {
            eyeGaze = GetComponent<OVREyeGaze>();

            faceExpressions = GetComponent<OVRFaceExpressions>();

            
            //Setup the attention map (just one for now)
            //gazeMap = new GazeMap();

            sortedHitsByLayer = new List<List<RaycastHit>>();

            prevSortedHitsByLayer = new List<List<RaycastHit>>();
        }
        void Start()
        {
            //nothing for now
            


        }

        /// <summary>
        /// Handles the inputs for this class, 
        /// </summary>
        private void HandleInputs()
        {
            if (Input.GetKeyDown(toggleRayCastKeyCode))
            {
                //swap
                castEyeRays = !castEyeRays;

            }

            //consider if this class should modify heatmaps or not
            //PORT THIS TO THE NEW VALUE 
            if (Input.GetKeyDown(heatMapModifyKeyCode))
            {
                //swap
                overlayHeatmap = !overlayHeatmap;
            }
            

        }

        /// <summary>
        /// Extracts the faceExpressions from the oculus SDK, and stores them in an array, 
        /// along with confidence weights for areas of the face
        /// </summary>
        private void GetFaceExpressions()
        {
            expressionsAreValid = faceExpressions.ValidExpressions;

            
            //check the expressions are valid
            if (faceExpressions.ValidExpressions)
            {

                //Get confidence values for upper alnd lower face regions (exposed to other classes)
                OVRFaceExpressions.FaceRegionConfidence lowerFace = OVRFaceExpressions.FaceRegionConfidence.Lower;
                OVRFaceExpressions.FaceRegionConfidence upperFace = OVRFaceExpressions.FaceRegionConfidence.Upper;
                faceExpressions.TryGetWeightConfidence(lowerFace, out lowerFaceConfidence);
                faceExpressions.TryGetWeightConfidence(upperFace, out upperFaceConfidence);

                float[] expressionArray = faceExpressions.ToArray();

                frameExpressions = expressionArray;
            }
        }

        /// <summary>
        /// Reset the expressions data 
        /// </summary>
        private void ResetExpressions() 
        {
            //assume false
            expressionsAreValid = false;
            //set array to to a set of zeros
            frameExpressions = new float[(int)OVRFaceExpressions.FaceExpression.Max];

            //set to -1 (invalid)
            lowerFaceConfidence = -1;
            upperFaceConfidence = -1;

        }

        /// <summary>
        /// called every frame, finds out what is hit, and gets eye and face data from the meta SDK
        /// </summary>
        private void Update()
        {
            //resets transforms and eye data 

            //moving new -> previous, needs to be made into a second Method that combines them all.
            ResetLayerLists();
            ResetAngularVelocities();
            ResetFrameTimes();
            ResetRotations();
            ResetExpressions();

            HandleInputs();


            SetRotations();
            GetFaceExpressions();
            
            //cast eye rays into environment if conditions are me 
            if (castEyeRays)
            {
                RaycastHit[] eyeRayHits = FindEyeRayHits();


                //return if nothing is hit (stuff is nulled)
                if (eyeRayHits.Length == 0) { return; }

                //Sort the list
                SortRaycastHits(eyeRayHits);
                

                sortedHitsByLayer = SplitHitsByLayer(eyeRayHits);

                                //get the gazePoints
                gazePointsByLayer = GenerateGazePointsFromHits(sortedHitsByLayer);
                //REMOVE THIS (for validating refactor works)

                if (gazePointsByLayer != null && gazePointsByLayer.Count != 0)
                {
                    UpdateAOI(gazePointsByLayer[0][0]);
                }

                ModifyHeatMap(eyeRayHits);

                LogAngularVelocities();
                //faceExpressions.
                LogEyePose();
            }

        }

        /// <summary>
        /// Tells the AOI script to add a gazePoint
        /// </summary>
        /// <param name="closestHit"> the gazePoint that is the closest hit</param>
        private void UpdateAOI(GazePoint closestHit) 
        {
            //handle null/empty errot

            if (closestHit == null) 
            { return; }

            GameObject hitObject = closestHit.GazeRayHit.collider.gameObject;
            AreaOfInterestScript AOIScript = hitObject.GetComponent<AreaOfInterestScript>();
            if (AOIScript != null)
            {
                //add the new gazePoint to that AOI object
                AOIScript.AddGazePoint(closestHit);
            }

        }

        /// <summary>
        /// Store if the eyes are open or not
        /// </summary>
        private void LogEyePose() 
        {
            if (faceExpressions.ValidExpressions)
            {
                float expressionWeight = -1.0f;
                if (isLeft)
                {
                    OVRFaceExpressions.FaceExpression expression = OVRFaceExpressions.FaceExpression.EyesClosedL;
                    //faceExpressions.GetEnumerator();
                    expressionWeight = 0.0f;
                    faceExpressions.TryGetFaceExpressionWeight(expression, out expressionWeight);
                }
                else 
                {
                    //isright
                    OVRFaceExpressions.FaceExpression expression = OVRFaceExpressions.FaceExpression.EyesClosedR;
                    expressionWeight = 0.0f;
                    faceExpressions.TryGetFaceExpressionWeight(expression, out expressionWeight);
                }

                eyeClosedConfidence = expressionWeight;

            }
            else
            {
                //invalid expressions
                Debug.Log("Invalid Expressions");
            }

        }


        /// <summary>
        /// Prepares the layerlists for the new frame: does the following
        /// 1. Stores the previous frames list of layer hits in prevSortedHitsByLayer
        /// 2. Makes the current frames sortedHitsByLayer null;
        /// CALL AT THE START OF UPDATE();
        /// </summary>
        private void ResetLayerLists()
        {
            //update the hit lists (Currently exposing redundant data???)
            prevSortedHitsByLayer = sortedHitsByLayer;
            sortedHitsByLayer = null;

            prevGazePointsByLayer = gazePointsByLayer;
            gazePointsByLayer = null;

        }

        /// <summary>
        /// 1. Stores the previous frame's eye and head angular velocities, then  
        /// CALL AT THE START OF UPDATE();
        /// </summary>
        private void ResetAngularVelocities() 
        {
            prevEyeAngularVelocity = newEyeAngularVelocity;

            newEyeAngularVelocity = -1.0f;

            prevHeadAngularVelocity = newHeadAngularVelocity;

            newHeadAngularVelocity = -1.0f;

        }
        
        /// <summary>
        /// Updates the frametimes of the prev/new frames
        /// </summary>
        private void ResetFrameTimes() 
        {
            prevFrameTime = newFrameTime;
            newFrameTime = -1.0f; //invalid value
        }

        /// <summary>
        /// Resets the stored values for rotations, but cannot do so the same way as other Reset methods do that others do as quaternion
        /// the new rotations cannot be nulled, so the values in the quaternions are set to infinity instead
        /// this does not update the values of 
        /// </summary>
        private void ResetRotations() 
        {
            prevEyeRotation = newEyeRotation;

            //alternative to nulling
            newEyeRotation.x = Mathf.Infinity;
            newEyeRotation.y = Mathf.Infinity;
            newEyeRotation.z = Mathf.Infinity;
            newEyeRotation.w = Mathf.Infinity;

            prevHeadRotation = newHeadRotation;

            //alternative to nulling
            newHeadRotation.x = Mathf.Infinity;
            newHeadRotation.y = Mathf.Infinity;
            newHeadRotation.z = Mathf.Infinity;
            newHeadRotation.w = Mathf.Infinity;

        }

        /// <summary>
        /// Updates the stored values for the new rotation of the head and eyes
        /// </summary>
        private void SetRotations() 
        {
            newEyeRotation = this.transform.rotation;

            newHeadRotation = this.transform.rotation;

        }

        
        /// <summary>
        /// Modifies the heatmap on objects, this function should only be called on objects with the heatmapscript component
        /// </summary>
        /// <param name="eyeRayHits"></param>
        private void ModifyHeatMap(RaycastHit[] eyeRayHits)
        {
            
            //if (!modifyHeatMap) { return; }

            if (eyeRayHits.Length == 0) { return; }

            //testHeatmapModify(eyeRayHits);
            //My own version
            myHeatMapModify(eyeRayHits);

        }

        /// <summary>
        /// finds the intersection points (world space) of all correct object hits
        /// </summary>
        /// <returns>
        /// Returns a non-sorted list of RaycastHits from the eyeray on the given layers
        /// </returns>
        private RaycastHit[] FindEyeRayHits()
        {
            //Get the direction vector for the eyeRay
            Vector3 eyeRayDirection = transform.TransformDirection(Vector3.forward) * rayLength;

            //list of all raycast hits 
            //Ray eyeRay = new Ray(eyePos, eyeRayDirection);
            Ray eyeRay = new Ray(transform.position, eyeRayDirection);


            //finds the number of hits (numHits) of a raycast on layermasks layersToCollide (specified on raycaster in unity)
            //NOTE Lower performance, IF PERFORMANCE BECOMES AN ISSUE USE  Physics.RaycastNonAlloc (LESS GC)
            RaycastHit[] eyeRayHits = Physics.RaycastAll(eyeRay, Mathf.Infinity, layersToCollide);

            return eyeRayHits;

        }

        /// <summary>
        /// Sort an ARRAY of raycastHits into increasing distance, using a lambda expression with raycastHit.distance as the comparison operator
        /// </summary>
        private void SortRaycastHits(RaycastHit[] hitsToSort)
        {
            //Lambda expression sort, compares the distance property of two RaycastHits (ascending order )
            Array.Sort(hitsToSort, (RaycastHit a, RaycastHit b) => (a.distance.CompareTo(b.distance)));
        }

        //Sort a LIST of raycastHits into increasing distance, CURRENTLY NOT USED
        private void SortRaycastHits(List<RaycastHit> hitsToSort)
        {
            //Lambda expression sort, compares the distance property of two RaycastHits (ascending order )
            hitsToSort.Sort((RaycastHit a, RaycastHit b) => (a.distance.CompareTo(b.distance)));
        }

        /// <summary>
        /// Splits a list of raycast Hits into lists of each layer hit (THIS FRAME)
        /// </summary>
        /// <param name="hitList"></param>
        /// <returns></returns>
        private List<List<RaycastHit>> SplitHitsByLayer(RaycastHit[] hitList)
        {

            //Guard against empty hit list
            if (hitList.Length == 0) { throw new Exception("SplitHitsByLayer Passed empty list of hits"); }

            List<List<RaycastHit>> sortedHitsByLayer = new List<List<RaycastHit>>();

            //append closest hit 
            foreach (RaycastHit hit in hitList)
            {
                //Find what the rayCastHit hit
                GameObject hitObject = hit.collider.gameObject;
                //Flag to see if we have found this layer in the list list already
                bool foundLayer = false;
                int hitObjectLayer = hitObject.layer;

                //layerOfHits is a layer of hits by hits
                foreach (List<RaycastHit> hitLayer in sortedHitsByLayer)
                {
                    //guaranteed to have atleast one hit in each sublist, 
                    int queryLayer = hitLayer[0].collider.gameObject.layer;

                    //if the layer of the object is the same layer as this sublist then append it
                    //IF the array is ordered, each sublist will be itself ordered by layer

                    if (hitObjectLayer == queryLayer)
                    {
                        hitLayer.Add(hit);
                        //set flag as A layer has been found and the hit has been appended
                        foundLayer = true;
                        //Break outof loop as the hit has been placed
                        break;
                    }

                }

                //If no hits stored have the same layer as this one, then a new list of raycast hits is needed for that new layer
                if (!foundLayer)
                {
                    //create new list of RaycastHit for an unseen layer (in the parse)
                    List<RaycastHit> newHitLayer = new List<RaycastHit>();
                    newHitLayer.Add(hit);
                    sortedHitsByLayer.Add(newHitLayer);
                }

            }
            return sortedHitsByLayer;
        }
        
        /// <summary>
        /// Generates a list of gazePoints split by layer
        /// </summary>
        /// <param name="sortedHits"> a sorted list of raycastHits, split by layer, ascending in distance</param>
        /// <returns></returns>
        private List<List<GazePoint>> GenerateGazePointsFromHits(List<List<RaycastHit>> sortedHits)
        {

            if (sortedHits == null)
            {
                return null;
            }

            List<List<GazePoint>> gpByLayers = new List<List<GazePoint>>();
            foreach (List<RaycastHit> hitList in sortedHits)
            {
                if (hitList != null)
                {
                    List<GazePoint> gpList = new List<GazePoint>();
                    gpByLayers.Add(gpList);

                    //The list should be sorted,
                    //Therfore first element in each layer's list will be the only one we care about (closest)
                    RaycastHit hit = hitList[0];
                    GazePoint newGP = CreateGazePoint(hit);
                    gpList.Add(newGP);
                }
            }

            return gpByLayers;
        }

        /// <summary>
        /// Creates a gazePoint, fetching and storing the necessary parameters.
        /// </summary>
        /// <param name="closestHit"> Should be the closest hit on a layer </param>
        /// <returns>
        /// Returns the gazePoint after creation
        /// </returns>
        private GazePoint CreateGazePoint(RaycastHit hit)
        {
            //store camera and hit Position in gazePoint
            Vector3 camPos = GetCameraPosition();
            GazePoint newGazePoint = new GazePoint(hit, camPos);

            return newGazePoint;

        }

        void myHeatMapModify(RaycastHit[] sortedEyeRayHit)
        {

            RaycastHit bestRaycastHit = sortedEyeRayHit[0];

            GameObject hitObject = bestRaycastHit.collider.gameObject;

            HeatMapScript hitHs = hitObject.GetComponent<HeatMapScript>();
            if (hitHs != null)
            {
              
                //maps between 0-1
                //Maps to texture space 
                hitHs.AddHitPoint(bestRaycastHit.textureCoord.x * 4 - 2, bestRaycastHit.textureCoord.y * 4 - 2); 

                if (overlayHeatmap)
                {
                    hitHs.overLayHeatMap = 1;
                }
                else 
                {
                    hitHs.overLayHeatMap = 0;
                }

            }


        }

        //gets the central point for the head
        public Vector3 GetCameraPosition()
        {
            return HeadAnchor.transform.position;
        }

        /// <summary>
        /// Deprecated, but will display the debug camera position
        /// </summary>
        /// <param name="gp"></param>
        private void ShowDebugCameraPosition(GazePoint gp)
        {
            //Debug.Log("Attaching debug camera");
            
            gp.cameraSphere = Instantiate(cameraPointSphere, gp.CameraWorldPosition, Quaternion.identity);
        }


        /// <summary>
        /// Deprecated: finds the closest raycastHit in all layers tracked , 
        /// </summary>
        private RaycastHit findClosestRaycastHit(RaycastHit[] allHits)
        {
            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;

            foreach (RaycastHit hit in allHits)
            {
                if (hit.distance < closestHit.distance)
                {
                    closestHit = hit;
                }
            }
            return closestHit;
        }

        /// <summary>
        /// Logs the angular velocities of the eye
        /// </summary>
        private void LogAngularVelocities() 
        {

            //update new frame time (current)
            newFrameTime = Time.time;

            float timeDiff = (newFrameTime - prevFrameTime);

            //update the angular velocity for the eyes
            float eyeAbsAngle = Quaternion.Angle(prevEyeRotation,newEyeRotation);
            float eyeAngularVelocity = eyeAbsAngle / timeDiff;

            newEyeAngularVelocity = eyeAngularVelocity;

            //update the angular velocity for the head
            float headAbsAngle = Quaternion.Angle(prevHeadRotation, newHeadRotation);
            float headAngularVelocity = headAbsAngle / timeDiff;

            newHeadAngularVelocity = headAngularVelocity;
        }

    }
}
