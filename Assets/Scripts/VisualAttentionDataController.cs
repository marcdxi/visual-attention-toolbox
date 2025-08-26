using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
//using static VisualAttentionToolbox.GazeAnalysisUtilities;
using static VisualAttentionToolbox.GazeAnalysisUtilities;


namespace VisualAttentionToolbox
{

    /// <summary>
    /// Main logical pathway through, the toolbox. 
    /// Handles all visual attention data
    /// </summary>
    public class VisualAttentionDataController : MonoBehaviour
    {

        [SerializeField]
        KeyCode logToFile = KeyCode.Backspace;

        [SerializeField]
        KeyCode toggleGazeVisualisations = KeyCode.I;

        [SerializeField]

        KeyCode detectSaccadesKey = KeyCode.P;

        [SerializeField]

        private SaccadeDetector saccadeDetector;

        [SerializeField]

        KeyCode detectSPKey = KeyCode.L;

        [SerializeField]
        private LineRenderer lineRenderer;



        [SerializeField]
        //debug Visualisations; 
        public bool showDebugVisualisations = false;

        // Start is called before the first frame update


        //TODO, Port this to be the older version
        [SerializeField]
        EyeRayCast leftEyeCaster = null;
        [SerializeField]
        EyeRayCast rightEyeCaster = null;

        //GazeMap
        [SerializeField]
        public GazeMap gazeMap;

        //BEGIN::Fixational variables
        [SerializeField]
        public FixationManager fixationManager = null;

        [SerializeField]
        public FixationIdentifier fixationIdentifier = null;

        //This needs to be abstracted away for other fixationIdentifiers to be used (TODO???)
        //max incoming velocity for two gaze points to be in same fixation;
        [SerializeField]
        public float IVThreshold = 3.0f;
        //END::Fixational variables


        //BEGIN::Debug/Visualisation Spheres
        public GameObject gazePointSphere;
        public GameObject cameraPointSphere;
        public GameObject fixationCentroidSphere;
        //END:: Debug/Visualisation Spheres
        public GameObject linePrefab;
        public Material smoothPursuitMaterial;

        //BEGIN::Reference Objects (Stuff this script needs access to)
        [SerializeField]
        private GameObject HeadAnchor;
        //END:: Reference objects

        //Consider moving this to its own class??
        public List<VisualAttentionFrameData> VATFrameData;

        public List<FaceExpressionsFrameData> expressionsFrameData;

        //BEGIN:: Eye Velocity information
        private float leftEyeAngluarVelocity;


        public AreaOfInterestManager AOIManager;

        public KeyCode DetectSaccadesKey { get => DetectSaccadesKey1; set => DetectSaccadesKey1 = value; }
        public KeyCode DetectSaccadesKey1 { get => detectSaccadesKey; set => detectSaccadesKey = value; }
        public KeyCode DetectSaccadesKey2 { get => detectSaccadesKey; set => detectSaccadesKey = value; }

        //     List<GazePoint> leftEyeStored = null;
        //   List<GazePoint> rightEyeStored = null;

        private void HandleAwakeErrors()
        {
            //Use case??
            if (leftEyeCaster == null)
            {
                throw new System.Exception("leftEyeCaster has not been set in VisualAttentionDataController");
            }

            if (rightEyeCaster == null)
            {
                throw new System.Exception("RightEyeCaster has not been set in VisualAttentionDataController");
            }

            if (fixationIdentifier == null)
            {
                throw new System.Exception("STARTUP ERROR");

            }

        }



        //potentially useless..., consider if needed (could just load these int the needed objects instead
        private void AssignDebugSpheres()
        {
            gazePointSphere = Resources.Load<GameObject>("VisualisationSpheres/GazePointSphere");
            cameraPointSphere = Resources.Load<GameObject>("VisualisationSpheres/CameraPointSphere");
            fixationCentroidSphere = Resources.Load<GameObject>("VisualisationSpheres/FixationCentroidSphere");
        }

        /// <summary>
        /// Returns the position of the headAnchor (forehead point)
        /// </summary>
        /// <returns></returns>
        public Vector3 GetCameraPosition()
        {
            return HeadAnchor.transform.position;

        }


        /// <summary>
        /// Creates a blue debug CameraPointSphere in the 
        /// </summary>
        /// <param name="gp">GazePoint to display a cameraPointSphere for</param>
        private void ShowDebugCameraPosition(GazePoint gp)
        {


            gp.cameraSphere = Instantiate(cameraPointSphere, gp.CameraWorldPosition, Quaternion.identity);

            var renderer = gp.cameraSphere.GetComponent<Renderer>();

            renderer.enabled = showDebugVisualisations;


        }


        /// <summary>
        /// Deprecated: do not use,
        /// THIS METHOD HAS ISSUES, IT DOES NOT CONSIDER TEMPORAL DIMENSION (IMPORTANT), BEING RE-CODED NOW
        /// </summary>
        /// <param name="sortedHits"></param>
        /// <returns>
        /// </returns>

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
        /// Deprecated Stores the eyeHits (DONT DO IT THIS WAY), 
        /// </summary>
        /// <param name="sortedEyeHitsByLayer"></param>
        /// <param name="prevSortedEyeHitsByLayer"></param>
        /// 
        private void StoreEyeHits(List<List<RaycastHit>> sortedEyeHitsByLayer, List<List<RaycastHit>> prevSortedEyeHitsByLayer)
        {
            foreach (List<RaycastHit> layerHits in sortedEyeHitsByLayer)
            {

                if (layerHits != null)
                {
                    RaycastHit closestHit = layerHits[0];
                    GazePoint newGazePoint = CreateGazePoint(closestHit);

                    //assign the instantiated sphere to the gazePoint
                    //TODO (check if should instantiate this)
                    newGazePoint.debugSphere = Instantiate(gazePointSphere, closestHit.point, Quaternion.identity);
                    //Instantiate(gazePointSphere, closestHit.point, Quaternion.identity);

                    //Add new gaze point to the list in the gazeMap
                    gazeMap.appendGazePoint(newGazePoint);

                    //TODO Add modes to enable/disable this
                    ShowDebugCameraPosition(newGazePoint);

                    GameObject hitObject = newGazePoint.GazeRayHit.collider.gameObject;

                    AreaOfInterestScript AOIScript = hitObject.GetComponent<AreaOfInterestScript>();
                    if (AOIScript != null)
                    {
                        //add the new gazePoint to that AOI object
                        //AOIScript.addGazePoint(newGazePoint);
                    }

                    //Handle edge case of first point
                    if (gazeMap.getTotalGazeMapSize() == 0)
                    {
                        gazeMap.HeadGazePoint = newGazePoint;
                    }


                    //list of potentially important gaze points for the fixation algorithm
                    List<List<GazePoint>> prevGazePointsByLayer = GenerateGazePointsFromHits(prevSortedEyeHitsByLayer);

                    //LogFixation(newGazePoint, prevGazePointsByLayer);

                }

            }

        }

        /// <summary>
        /// Stores gazePoints in the GazeMap, and handles gazePoint visualisations , Does not add fixations to the scene
        /// </summary>
        /// <param name="sortedGazePointsByLayer"></param>
        /// <param name="prevSortedGazePointsByLayer">Can remove this???</param>
        private List<GazePoint> StoreGazePoints(List<List<GazePoint>> sortedGazePointsByLayer, List<List<GazePoint>> prevSortedGazePointsByLayer)
        {
            //list of gazePoints added to s
            List<GazePoint> stored = new();

            foreach (List<GazePoint> layerGazePoints in sortedGazePointsByLayer)
            {

                if (layerGazePoints != null)
                {
                    GazePoint newGazePoint = layerGazePoints[0];

                    //assign the instantiated sphere to the gazePoint

                    //if  (display spheres, do that)
                    newGazePoint.debugSphere = Instantiate(gazePointSphere, newGazePoint.WorldSpacePosition, Quaternion.identity);

                    //get the renderer, and make sure to show/hide it based on the debugspheres visualisations.
                    var renderer = newGazePoint.debugSphere.GetComponent<Renderer>();

                    renderer.enabled = showDebugVisualisations;

                    //Add new gaze point to the list in the gazeMap
                    gazeMap.appendGazePoint(newGazePoint);
                    //TODO Add modes to enable/disable this
                    ShowDebugCameraPosition(newGazePoint);
                    GameObject hitObject = newGazePoint.GazeRayHit.collider.gameObject;

                    AreaOfInterestScript AOIScript = hitObject.GetComponent<AreaOfInterestScript>();
                    //add the new gazePoint to that AOI object
                    if (AOIScript != null)
                    {
                        //AOIScript.addGazePoint(newGazePoint);
                    }

                    //Handle edge case of first point
                    if (gazeMap.getTotalGazeMapSize() == 0)
                    {
                        gazeMap.HeadGazePoint = newGazePoint;
                    }

                    stored.Add(newGazePoint);
                    //LogFixation(newGazePoint, prevSortedGazePointsByLayer);

                }

            }

            return stored;
        }


        // <summary>
        /// Searches gazePointsByLayer till it finds gazePoints that contain the layer supplied with the query layer
        /// </summary>
        /// <param name="queryLayer"></param>
        /// <returns>
        /// returns the index for the sublist of gazepoints if gazePoints of that layer have been found, 
        /// else returns -1;
        /// </returns>
        /// <remarks> 
        /// This code is copied from another class, consider moving to a utility function class, but this is potentially problematic
        /// </remarks>
        private int FindLayerIndex(int queryLayer, List<List<GazePoint>> toQuery)
        {
            if (toQuery == null)
            {
                return -1;
            }

            int foundLayerIndex = 0;
            foreach (List<GazePoint> gazePointLayer in toQuery)
            {

                //if an empty list is found, append the gazePoint to that layer
                if (gazePointLayer.Count == 0)
                {
                    return foundLayerIndex;
                }

                //query the first item in this layer
                int thisLayer = gazePointLayer[0].Layer;

                if (thisLayer == -1) { throw new Exception("FindLayerIndex found that GazePoint was not assigned layer correctly"); }

                //check if this layer contains gazepoints on that layer
                if (thisLayer == queryLayer)
                {
                    return foundLayerIndex;
                }
                else
                {
                    //invariant???
                    foundLayerIndex++;
                }

            }
            //Didnt find anything so return -1;
            return -1;
        }


        /// <summary>
        /// Deprecated, do not use
        /// </summary>
        /// <param name="newLeftGazePointsByLayer"></param>
        /// <param name="newRightGazePointsByLayer"></param>
        /// <param name="prevLeftGazePointsByLayer"></param>
        /// <param name="prevRightGazePointsBylayer"></param>
        //HORRIBLE FUNCTION SIGNATURE, WAY TOO LONG
        private void FindFixationGazePointToQuery(List<List<GazePoint>> newLeftGazePointsByLayer, List<List<GazePoint>> newRightGazePointsByLayer,
                                List<List<GazePoint>> prevLeftGazePointsByLayer, List<List<GazePoint>> prevRightGazePointsBylayer)
        {

            List<List<GazePoint>> applicableGazePointsByLayer = new List<List<GazePoint>>();

            foreach (List<GazePoint> gazeLayer in newLeftGazePointsByLayer)
            {
                //if (ga)

                if (gazeLayer == null)
                {
                    throw new Exception("GazeLayer is Null :(");

                }

                GazePoint leftNewGp = gazeLayer[0];
                if (leftNewGp == null) { throw new Exception("leftNewGp is null"); }


                //search for a 
                int queryLayer = leftNewGp.Layer;

                int leftPrevIndex = FindLayerIndex(queryLayer, prevLeftGazePointsByLayer);

                int rightNewIndex = FindLayerIndex(queryLayer, newRightGazePointsByLayer);

                int rightPrevIndex = FindLayerIndex(queryLayer, prevRightGazePointsBylayer);

                //Now determine if any of these are null;

                GazePoint leftPrevGp = FindGazePointByLayerIndex(leftPrevIndex, prevLeftGazePointsByLayer);
                GazePoint rightNewGp = FindGazePointByLayerIndex(rightNewIndex, newRightGazePointsByLayer);
                GazePoint rightPrevGp = FindGazePointByLayerIndex(rightPrevIndex, prevRightGazePointsBylayer);

            }
        }


        private GazePoint FindGazePointByLayerIndex(int layerIndex, List<List<GazePoint>> toQuery)
        {
            GazePoint foundGp = null;
            if (layerIndex != -1)
            {
                foundGp = toQuery[layerIndex][0];
                if (foundGp == null) { throw new Exception("foundGP is null"); }
            }
            return foundGp;
        }

        //Deprecated::Consider moving this to the fixationManager (Done)
        private void LogFixation(GazePoint newGazePoint, List<List<GazePoint>> prevGazePointsByLayer, float incomingAngularVelocity)
        {
            //search for a gazePoint with this layer, in the list of gazePoints

            //search for a 
            int queryLayer = newGazePoint.Layer;
            int foundLayerIndex = FindLayerIndex(queryLayer, prevGazePointsByLayer);

            if (foundLayerIndex == -1)
            {
                //nothing found 
            }
            else
            {
                //Gazepoint we care about for fixational calculations

                //first element of foundLayerIndex sublist is the applicable gazePoint
                GazePoint prevGazePoint = prevGazePointsByLayer[foundLayerIndex][0];

                //Debug.Log("Previous gaze point found with data");
                //Debug.Log(prevGazePoint);

                //consider the new gazePoint using the previousGP and fixationIdentifier of the fixationManager
                //takes in all the data that can be needed for fixational calculations, but does so in a way that 
                fixationManager.ConsiderNewGazePoint(prevGazePoint, newGazePoint, incomingAngularVelocity);


            }



        }
        /// <summary>
        /// Logs a fixations in the fixationManager
        /// </summary>
        /// <param name="newLeft"></param>
        /// <param name="prevLeftByLayer"></param>
        /// <param name="newRight"></param>
        /// <param name="prevRightByLayer"></param>
        /// <param name="leftIV"></param>
        /// <param name="rightIV"></param>
        private void LogFixation(GazePoint newLeft, List<List<GazePoint>> prevLeftByLayer,
                                 GazePoint newRight, List<List<GazePoint>> prevRightByLayer, float leftIV, float rightIV)
        {
            //search for a gazePoint with this layer, in the list of gazePoints

            //newLeft.Layer if not null else -1;
            int leftQueryLayer = (newLeft != null) ? newLeft.Layer : -1;
            int leftFoundLayerIndex = FindLayerIndex(leftQueryLayer, prevLeftByLayer);

            GazePoint prevLeft = null;
            if (leftFoundLayerIndex != -1) { prevLeft = prevLeftByLayer[leftFoundLayerIndex][0]; }

            //newRight.Layer if not null else -1;
            int rightQueryLayer = (newRight != null) ? newRight.Layer : -1;
            int rightFoundLayerIndex = FindLayerIndex(rightQueryLayer, prevRightByLayer);

            GazePoint prevRight = null;
            if (rightFoundLayerIndex != -1) { prevRight = prevRightByLayer[rightFoundLayerIndex][0]; }

            fixationManager.ConsiderNewGazePoint(prevLeft, prevRight, newLeft, newRight, leftIV, rightIV);

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


        private void Awake()
        {
            //potentially OverKill

            //TODO:: consider if necessary
            AssignDebugSpheres();

            OVREyeGaze eyeGaze = GetComponent<OVREyeGaze>();

            //Setup the gaze map (just one for now)
            gazeMap = new GazeMap();

            //TODO: REMOVE (FOR TESTING)
            //fixationIdentifier = new IVTSpatial(IVThreshold);

            fixationIdentifier = new IVTAngular(IVThreshold);


            //create a fixation manager using the desired fixationIdentifier algorithm
            fixationManager = new FixationManager(fixationIdentifier, this);

            VATFrameData = new List<VisualAttentionFrameData>();

            expressionsFrameData = new List<FaceExpressionsFrameData>();

            AOIManager = new AreaOfInterestManager();

            GameObject saccadeDetectorObject = new GameObject("SaccadeDetector");

            SaccadeDetector saccadeDetector = saccadeDetectorObject.AddComponent<SaccadeDetector>();

            if (lineRenderer == null)
            {
                GameObject lineRendererObject = new GameObject("SaccadeLineRenderer");
                lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
                lineRenderer.widthMultiplier = 0.01f; // Adjust line thickness 
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
            }



            HandleAwakeErrors();

        }

        /// <summary>
        /// Stores the important information for visual attentionin this frame for ease of output
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
        /// <remarks> basically a copied version of the constructor for the struct, but </remarks>
        private void StoreVisualAttentionFrameData(GazePoint passedLeftEyeGazePoint, GazePoint passedRightEyeGazePoint, float leftEyeOpenConfidence,
                                    float rightEyeOpenConfidence, Vector3 passedHeadCameraPosition, Vector3 passedLeftEyePos,
                                    Vector3 passedRightEyePos, int passedFrameCount, bool passedIsFixation, bool passedIsConjugate,
                                    float passedIncomingLeftEyeAngularVelocity, float passedIncomingRightEyeAngularVelocity,
                                    float passedIncomingHeadAngularVelocity)
        {
            VisualAttentionFrameData thisFrame = new VisualAttentionFrameData(passedLeftEyeGazePoint, passedRightEyeGazePoint, leftEyeOpenConfidence,
                                    rightEyeOpenConfidence, passedHeadCameraPosition, passedLeftEyePos,
                                    passedRightEyePos, passedFrameCount, passedIsFixation, passedIsConjugate,
                                    passedIncomingLeftEyeAngularVelocity, passedIncomingRightEyeAngularVelocity,
                                    passedIncomingHeadAngularVelocity);
            VATFrameData.Add(thisFrame);
            //Debug.Log(("Size of FrameData after adding = ", VATFrameData.Count, "frameCount = ", Time.frameCount));

        }


        /// <summary>
        /// Stores face expressions data per frame in an easily reaable format for output to csv
        /// </summary>
        /// <param name="frameCount"></param>
        /// <param name="time"></param>
        /// <param name="validExpressions"></param>
        /// <param name="expressions"></param>
        /// <param name="upperConfidence"></param>
        /// <param name="lowerConfidence"></param>
        private void StoreExpressionsFrameData(int frameCount, float time, bool validExpressions, float[] expressions, float upperConfidence, float lowerConfidence)
        {
            FaceExpressionsFrameData thisFrameExpressions = new FaceExpressionsFrameData(frameCount, time, validExpressions, expressions, upperConfidence, lowerConfidence);

            expressionsFrameData.Add(thisFrameExpressions);
        }

        void Start()
        {
            //dont know what to put here


        }

        /// <summary>
        /// Finds the first gaze Point in each layer, or returns null if it doesnt exist
        /// </summary>
        /// <param name="gazePointsByLayer"></param>
        /// <returns>the gazePoint if it exists, or null if it doesnt</returns>
        private GazePoint GetClosestGazePointByLayer(List<List<GazePoint>> gazePointsByLayer)
        {
            GazePoint first = null;

            if (gazePointsByLayer != null && gazePointsByLayer[0] != null)
            {
                first = gazePointsByLayer[0][0];
            }

            return first;
        }

        /// <summary>
        /// returns true if left or right gazePoints have a fixationID assigned != -1 (Unset)
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private bool FrameIsFixational(GazePoint left, GazePoint right)
        {
            //
            if ((left != null) && (left.fixationID != -1)) { return true; }

            //
            if ((right != null) && (right.fixationID != -1)) { return true; }

            return false;

        }

        /// <summary>
        /// Returns true if the two gazePoints are conjugate.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private bool FrameIsConjugate(GazePoint left, GazePoint right)
        {
            if ((left != null) && (right != null))
            {
                //check if both objects have same reference (and are therefore identical) 
                if (ReferenceEquals(left.GazeRayHit.collider.gameObject, right.GazeRayHit.collider.gameObject))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        // Update is called once per frame
        //THIS SCRIPT MUST RUN after eyeRayCasts to work, the execution order in the unity environment must be set to run VisualAttentionDataController after the raycasting scripts
        void Update()
        {

            List<List<GazePoint>> prevLeftEyeGazePointsByLayer = leftEyeCaster.PrevGazePointsByLayer;
            List<List<GazePoint>> newLeftEyeGazePointsByLayer = leftEyeCaster.GazePointsByLayer;

            List<GazePoint> leftEyeStored = null;

            if (newLeftEyeGazePointsByLayer != null)
            {
                leftEyeStored = StoreGazePoints(newLeftEyeGazePointsByLayer, prevLeftEyeGazePointsByLayer);

            }


            List<List<GazePoint>> prevRightEyeGazePointsByLayer = rightEyeCaster.PrevGazePointsByLayer;
            List<List<GazePoint>> newRightEyeGazePointsByLayer = rightEyeCaster.GazePointsByLayer;

            float rightEyeAngularVelocity = rightEyeCaster.NewEyeAngularVelocity;

            List<GazePoint> rightEyeStored = null;
            if (newRightEyeGazePointsByLayer != null)
            {
                rightEyeStored = StoreGazePoints(newRightEyeGazePointsByLayer, prevRightEyeGazePointsByLayer);
            }

            // Debug.Log("VisualAttentionDataController finished running");

            //handle left eye hits first

            float leftEyeAngularVelocity = leftEyeCaster.NewEyeAngularVelocity;

            if (leftEyeAngularVelocity != -1) //not unset
            {
                //Debug.Log(("left eye angular speed = leftEyeAngularVelocity", leftEyeAngularVelocity));
            }

            GazePoint newLeft = null;
            if (leftEyeStored != null)
            {
                newLeft = leftEyeStored[0];
            }

            GazePoint newRight = null;
            if (rightEyeStored != null)
            {
                newRight = rightEyeStored[0];

            }

            LogFixation(newLeft, prevLeftEyeGazePointsByLayer, newRight, prevRightEyeGazePointsByLayer, leftEyeAngularVelocity, rightEyeAngularVelocity);


            //Move this logic to another method at some point

            GazePoint closestLeft = GetClosestGazePointByLayer(newLeftEyeGazePointsByLayer);

            GazePoint closestRight = GetClosestGazePointByLayer(newRightEyeGazePointsByLayer);

            //mean taken of two samples should give more accurate result
            float headAngularVelocity = (leftEyeCaster.NewHeadAngularVelocity + rightEyeCaster.NewHeadAngularVelocity) / 2.0f;

            StoreVisualAttentionFrameData(closestLeft, closestRight, leftEyeCaster.EyeClosedConfidence, rightEyeCaster.EyeClosedConfidence, leftEyeCaster.GetCameraPosition(), leftEyeCaster.transform.position,
                rightEyeCaster.transform.position, Time.frameCount, FrameIsFixational(closestLeft, closestRight), FrameIsConjugate(closestLeft, closestRight), leftEyeCaster.NewEyeAngularVelocity,
                rightEyeCaster.NewEyeAngularVelocity, headAngularVelocity);

            //End moving

            //Using left eye arbitrarily as a face data should be equivalient between eyes (accesses same datastream)
            StoreExpressionsFrameData(Time.frameCount, Time.time, leftEyeCaster.ExpressionsAreValid, leftEyeCaster.FrameExpressions, leftEyeCaster.UpperFaceConfidence, leftEyeCaster.LowerFaceConfidence);


            //Log data to files
            if (Input.GetKeyDown(logToFile))
            {
                //disable casting eye rays
                leftEyeCaster.castEyeRays = false;
                rightEyeCaster.castEyeRays = false;

                FileWriter fr = new FileWriter();

                fr.WriteAllGazePointsToFile(this);

                fr.LogVisualAttentionFrameDataToFile(this);

                fr.LogExpressionFrameDataToFile(this);

                fr.LogAOIInfoToFile(this);

                fr.LogFixationsToFile(this);

            };

            //Need to make rebindable
            if (Input.GetKeyDown(toggleGazeVisualisations))
            {
                //toggle
                showDebugVisualisations = !showDebugVisualisations;

                //disable/enable based on the showDebugVisualisations bool
                gazeMap.ShowGazeVisualisations(showDebugVisualisations);

                fixationManager.ShowFixationVisualisations(showDebugVisualisations);

            }

            if (Input.GetKeyDown(detectSPKey))
            {
                Debug.Log(("size of fixationList for SPs are: ", fixationManager.FixationList.Count));
                Debug.Log(("Count of right eye gaze points: ", rightEyeStored.Count));
                detectSP2();
            }

            void detectSmoothPursuits()
            {
                List<SmoothPursuit> pursuits = new List<SmoothPursuit>();
                List<GazePoint> allGazePoints = gazeMap.allGazePoints; // Assuming direct access is available

                for (int i = 0; i < fixationManager.FixationList.Count - 1; i++)
                {
                    Fixation currentFixation = fixationManager.FixationList[i];
                    Fixation nextFixation = fixationManager.FixationList[i + 1];

                    List<GazePoint> inBetweenGazePoints = GazeAnalysisUtilities.GetGazePointsBetween(currentFixation, nextFixation, allGazePoints);
                    Debug.Log("inbetweengazepoints: " + inBetweenGazePoints.Count);

                    if (GazeAnalysisUtilities.IsSmoothPursuit(inBetweenGazePoints))
                    {
                        SmoothPursuit pursuit = new SmoothPursuit(currentFixation, nextFixation, inBetweenGazePoints);
                        pursuits.Add(pursuit);
                    }
                }

                foreach (var pursuit in pursuits)
                {
                    Debug.Log($"Detected Smooth Pursuit from fixation {pursuit.StartFixation.ID} to {pursuit.EndFixation.ID}");
                    DrawPursuitLine(pursuit);
                }

                if (pursuits.Count == 0)
                {
                    Debug.Log("No Detected Smooth Pursuit movement");
                }
            }

            //void DrawPursuitLine(SmoothPursuit pursuit)
            //{
            //    if (pursuit.GazePoints.Count < 2) return; // Need at least two points to draw a line

            //    GameObject pursuitLineObject = new GameObject("SmoothPursuitLine");
            //    LineRenderer pursuitRenderer = pursuitLineObject.AddComponent<LineRenderer>();

            //    pursuitRenderer.material = smoothPursuitMaterial;
            //    pursuitRenderer.startWidth = 0.02f;
            //    pursuitRenderer.endWidth = 0.02f;
            //    pursuitRenderer.positionCount = pursuit.GazePoints.Count;

            //    Vector3[] positions = pursuit.GazePoints.Select(gp => gp.WorldSpacePosition).ToArray();
            //    pursuitRenderer.SetPositions(positions);
            //}

            void DrawPursuitLine(SmoothPursuit pursuit)
            {
                if (fixationManager == null) return;

                // Find indices for start and end fixations in the list
                int startIndex = fixationManager.FixationList.IndexOf(pursuit.StartFixation);
                int endIndex = fixationManager.FixationList.IndexOf(pursuit.EndFixation);

                // Check if indices are valid
                if (startIndex == -1 || endIndex == -1 || startIndex > endIndex) return;

                GameObject pursuitLineObject = new GameObject("SmoothPursuitLine");
                LineRenderer pursuitRenderer = pursuitLineObject.AddComponent<LineRenderer>();

                pursuitRenderer.material = smoothPursuitMaterial; // Make sure this material is assigned and supports the rendering.
                pursuitRenderer.startWidth = 0.02f;
                pursuitRenderer.endWidth = 0.02f;
                pursuitRenderer.positionCount = endIndex - startIndex + 1; // Positions count is the number of fixations to connect

                Vector3[] positions = new Vector3[pursuitRenderer.positionCount];

                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] = fixationManager.FixationList[startIndex + i].CentroidPosition;
                }

                pursuitRenderer.SetPositions(positions);
            }

            void detectSP2()
            {
                Debug.Log("No Detected Smooth Pursuit movement");
                List<SmoothPursuit> smoothPursuits = new List<SmoothPursuit>();
                Debug.Log("Fixations: " + fixationManager.FixationList.Count);

                if (fixationManager.FixationList.Count < 2)
                {
                    Debug.Log("Not enough fixations");
                }

                SmoothPursuit currentSmoothPursuit = null;
                int minFixations = 5;
                float smoothPursuitDistanceThreshold = 5.0f;
                var fixations = fixationManager.FixationList;

                for (int i = 1; i < fixations.Count; i++)
                {
                    float distanceBetweenFixations = Vector3.Distance(fixations[i - 1].CentroidPosition, fixations[i].CentroidPosition);
                    Debug.Log("Distance: " + distanceBetweenFixations);

                    if (distanceBetweenFixations < smoothPursuitDistanceThreshold)
                    {
                        // If distance is less than the threshold, continue the current smooth pursuit
                        if (currentSmoothPursuit == null)
                        {
                            // Start a new smooth pursuit if it's the first pair of consecutive fixations within threshold
                            currentSmoothPursuit = new SmoothPursuit(fixations[i - 1], fixations[i], new List<GazePoint>());
                        }
                        else
                        {
                            // Add the current fixation to the ongoing smooth pursuit
                            currentSmoothPursuit.EndFixation = fixations[i];
                        }
                    }
                    else
                    {
                        // End the current smooth pursuit if the distance exceeds the threshold
                        float fixationSpan = currentSmoothPursuit.EndFixation.ID - currentSmoothPursuit.StartFixation.ID;
                        if (currentSmoothPursuit != null && fixationSpan > minFixations)
                        {
                            smoothPursuits.Add(currentSmoothPursuit);
                            currentSmoothPursuit = null;
                        }
                    }
                }

                // Add the last smooth pursuit if one is still ongoing
                if (currentSmoothPursuit != null)
                {

                }

                foreach (var pursuit in smoothPursuits)
                {
                    Debug.Log($"Detected Smooth Pursuit from fixation {pursuit.StartFixation.ID} to {pursuit.EndFixation.ID}");
                    DrawPursuitLine(pursuit);
                }

                return;
            }



            if (Input.GetKeyDown(detectSaccadesKey))
            {
   //             Debug.Log(("size of fixationList in fixationManager is", fixationManager.FixationList.Count));
                detectSaccades();
            }

            void detectSaccades()
            {
                List<Saccade> saccades = new List<Saccade>();
                if (fixationManager == null || fixationManager.FixationList == null) return;


                var fixations = fixationManager.FixationList;
                lineRenderer.positionCount = (fixations.Count - 1) * 2;
                int linePositionIndex = 0;

                for (int i = 0; i < fixations.Count - 1; i++)
                {
                    // Calculate the time between the end of one fixation and the start of the next
                    float timeBetweenFixations = fixations[i + 1].CentroidTime - (fixations[i].CentroidTime + fixations[i].Duration);

                    if (timeBetweenFixations <= 0.05f)
                    {
                        // A saccade is detected between the two fixations
                        Saccade saccade = new Saccade()
                        {
                            StartFixation = fixations[i],
                            EndFixation = fixations[i + 1],
                            StartTime = fixations[i].CentroidTime + fixations[i].Duration,
                            EndTime = fixations[i + 1].CentroidTime
                        };
                        saccades.Add(saccade);

                        lineRenderer.SetPosition(linePositionIndex++, fixations[i].CentroidPosition);
                        lineRenderer.SetPosition(linePositionIndex++, fixations[i + 1].CentroidPosition);

                        Debug.Log("Saccade found");

                    }
                }
                Debug.Log(saccades.Count);
                return;
            }

        }



    }
}
