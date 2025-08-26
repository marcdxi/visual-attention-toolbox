
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace VisualAttentionToolbox
{
 
    /// <summary>
    /// Outlines information stored about raw gaze data
    /// Including:
    /// Space position in worldspace and relative to an object
    /// time (seconds since program start), framecount
    /// raycastHit information (angle of incidence, object hit)
    /// layer
    /// and other
    /// </summary>

    public class GazePoint
    {

        public int frameCount 
        {
            get;
            private set;
        }

        //the time of a gaze hit
        public float gazeTime;

        //camera position of gaze hits
        private Vector3 cameraWorldPosition;

        public Vector3 CameraWorldPosition 
        {
            private set { cameraWorldPosition = value; } 

            get { return cameraWorldPosition; }
        
        }

        //Sphere for debugging
        public GameObject debugSphere;

        //displays view position the gazePoint was viewed from
        public GameObject cameraSphere;

        private Vector3 worldSpacePosition;

        public Vector3 WorldSpacePosition
        {
            //Set should ideally only be done at GazePoint creation time
            private set { worldSpacePosition = value;  }

            get { return worldSpacePosition; }

        }

        //SpacioTemporal Position for gaze point in world space, (needed for aggregation of points into fixations
        private Vector3 objectSpacePosition;

        public Vector3 ObjectSpacePosition
        {
            //Set should ideally only be done at GazePoint creation time
            private set { objectSpacePosition = value; }

            get { return objectSpacePosition; }

        }

        private Vector3 originalObjectOffset;

        public Vector3 OriginalObjectOffset
        {
            get;
        }

        //Should AT LEAST be, GazePoint intersection with eye ray, But also working
        //Hit information, should contain information like normals ect (duplicated information from gazePosition)
        private RaycastHit gazeRayHit;

        public RaycastHit GazeRayHit
        {
            //Set should ideally only be done at GazePoint creation time
            private set { gazeRayHit = value; }

            get { return gazeRayHit; }

        }

        //stores some ID value for fixation aggregation later (provided time permits
        public int fixationID = -1; //default to invalid value

        //Move to enum later. potentially tracking the type of gaze that led to this gaze point
        public int gazeType = -1;

        //Stores the layer that the gazePoint hit was found on, encapsulated, layer for a gazepoint should not change

        //could use readonly (only allow changing the layer through) (Default to error state of -1)
        private int layer = -1;
        public int Layer 
        {
            get { return layer; }

            private set { layer = value; }
        }

        //allows linking of gazepoints, could be useful with determining angular velocity/other stuff important
        //TODO: clarify if linked list necessary (slow, but should be fine given reasonable (ish) size of the 
        
        //REMOVE THIS
        public GazePoint nextGazePoint;
        public GazePoint prevGazePoint;

        //default constructor
        public GazePoint() 
        {
            gazeRayHit = new RaycastHit();
            nextGazePoint = null;
            prevGazePoint = null;
            worldSpacePosition = new Vector3();

            cameraWorldPosition = new Vector3();

            frameCount = Time.frameCount;

        }

        public GazePoint(RaycastHit hit)
        {
            gazeRayHit = hit;

            //Get spaceTime Position
            worldSpacePosition = hit.point;
            gazeTime = Time.time;
            
            objectSpacePosition = hit.collider.gameObject.transform.localPosition;

            gazeTime = Time.time;

            //set the layer
            this.Layer = hit.collider.gameObject.layer;

            frameCount = Time.frameCount;
        }

        //DEPRECATED, DELETE THIS LATER
        public GazePoint(RaycastHit hit, int hitLayer) 
        {
            gazeRayHit = hit;

            //Get spaceTime Position
            worldSpacePosition = hit.point;
            gazeTime = Time.time;

            objectSpacePosition = hit.collider.gameObject.transform.localPosition;

            //get the localOffset vector
            originalObjectOffset = hit.collider.gameObject.transform.localPosition;

            //set layer using setter
            this.Layer = hitLayer;

            frameCount = Time.frameCount;

        }

        public GazePoint(RaycastHit hit, Vector3 cameraPosition) 
        {
            gazeRayHit = hit;

            //Get spaceTime Position
            worldSpacePosition = hit.point;
            gazeTime = Time.time;

            objectSpacePosition = hit.collider.gameObject.transform.localPosition;

            this.Layer = hit.collider.gameObject.layer;

            this.cameraWorldPosition = cameraPosition;

            frameCount = Time.frameCount;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        public GazePoint(GazePoint other)
        {
            frameCount = other.frameCount;
            gazeTime = other.gazeTime;

            cameraWorldPosition = other.cameraWorldPosition;
            worldSpacePosition = other.WorldSpacePosition;
            objectSpacePosition = other.ObjectSpacePosition;
            layer = other.layer;
            fixationID = other.fixationID;
            gazeRayHit= other.gazeRayHit;
            nextGazePoint = other.nextGazePoint;
            prevGazePoint = other.prevGazePoint;
            gazeType = other.gazeType;
        }

        /// <summary>
        /// creates a gazepoint that is the centroid/average of two others
        /// information conflicts are given a left bias.
        /// </summary>
        //public GazePoint()
        public GazePoint CreateCentroid(GazePoint a, GazePoint b)
        {
            //
            //Need to find the average gazePoint of two 
            if (a == null) 
            {
                //return new GazePoint(b);
                return b;
            }

            if (b == null) 
            {
                //return new GazePoint(a);
                return a;
            }

            //A and B are both not null, so we can find a centroid of their data
            GazePoint centroid = new GazePoint();

            //assumes that both previous gazePoints are from the previous frame (else a bug exists )
            centroid.frameCount = a.frameCount;

            centroid.worldSpacePosition = (a.WorldSpacePosition + b.WorldSpacePosition) / 2.0f;

            //This is problematic, as in disconjugate movements the two gaze points will be in different positions.
            centroid.objectSpacePosition = a.ObjectSpacePosition;
            //TODO::Calculate object space position better.
            //if (a.GazeRayHit.collider.gameObject.name == b.GazeRayHit.collider.gameObject.name)

            centroid.cameraWorldPosition = (a.CameraWorldPosition + b.CameraWorldPosition) / 2.0f;

            //Layer is problematic like avgObjectspacePosition.
            centroid.layer = a.Layer;

            //Resolves to whichever one has a layer!=-1, priotitizing a
            centroid.fixationID = a.fixationID; 

            //This logic is the issue.
            if (a.fixationID != b.fixationID)
            {
                if (a.fixationID == -1)
                {
                    centroid.fixationID = b.fixationID; 
                }

            } 
            else 
            {
                centroid.fixationID = a.fixationID;
            }

            centroid.gazeRayHit =  a.gazeRayHit;

            //problematic, but cannot avoid this
            nextGazePoint = a.nextGazePoint;
            prevGazePoint = a.prevGazePoint;

            gazeType = a.gazeType;

            centroid.gazeTime = a.gazeTime;
            return centroid;
        }


        //stores the incoming rotation (previous angle -> this angle)
        public Quaternion incomingRotation;



        //Override for printing (helps with debug)
        public override string ToString()
        {
            return "GazePoint with Position: <" + worldSpacePosition.x + "," + worldSpacePosition.y + "," + worldSpacePosition.z + ">" + "At Time " + gazeTime +
                "\n Object Hit has name" + gazeRayHit.collider.name + "Object has hash "+ this.GetHashCode();
        }

        //updates the world position of the debugsphere (NOT WORKING)

        /// <summary>
        /// Deprecated, originally updated the position of a gazePoint visualistion as the object it lay on moved
        /// </summary>
        public void updateDebugSpherePosition() 
        {
            //what the gaze hit
            GameObject gazeObjectHit = gazeRayHit.collider.gameObject;

            Vector3 newWorldPosition = new Vector3();

            Vector3 oldObjectPosition = new Vector3();

            //
            oldObjectPosition.x = objectSpacePosition.x;
            oldObjectPosition.y = objectSpacePosition.y;
            oldObjectPosition.z = objectSpacePosition.z;


            newWorldPosition = gazeObjectHit.transform.InverseTransformPoint(oldObjectPosition);

            //set new world position of debugsphere
            this.debugSphere.transform.position = newWorldPosition;


        }
        
    }
}