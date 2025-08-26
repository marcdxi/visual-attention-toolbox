using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualAttentionToolbox
{


    /// <summary>
    /// Manager class for gaze information (gaze points) 
    /// </summary>
    public class GazeMap
    {

        //head for gazePoint (not decided on linked list or not yet)
        //Using c# best practises https://softwareengineering.stackexchange.com/questions/344893/is-there-a-better-way-of-using-getters-and-setters-on-private-members-of-a-class
        private GazePoint headGazePoint;

        public GazePoint HeadGazePoint
        { 
            get { return headGazePoint; }
            
            //
            set 
            {

                if (value.prevGazePoint != null)
                {
                    //if the value has a previous gazepoint it isnt valid.
                    headGazePoint = null;
                }
                else 
                {
                    //value is what we are setting (c# magic)
                    headGazePoint = value; 
                }
            }
                
        }
       
        //LIST of ALL list of all gazepoints (in the GazeMap)
        public List<GazePoint> allGazePoints;


        private List<List<GazePoint>> gazePointsByLayer;


        public List<List<GazePoint>> GazePointsByLayer 
        { 
            get { return gazePointsByLayer;  }

            private set { gazePointsByLayer = value; }
        }

        public GazePoint AllGazePoints(int index) 
        {
            if (index < 0)
            {
                return null;
                
            }

            GazePoint gp = allGazePoints[index];

            return gp;
            
        }


        public int gazeMapID;

        public GazeMap() 
        {
            //TODO: Decide soon on linked list or not
            headGazePoint = new GazePoint();

            allGazePoints = new List<GazePoint>();
            
            gazePointsByLayer = new List<List<GazePoint>>();

        }

        /// <summary>
        /// Adds the gazePoint onto the generic container (allGazePoints) 
        /// and into the correct subcontainer in gazePointsByLayer
        /// </summary>
        
        public void appendGazePoint(GazePoint gazePoint) 
        {   
            //first append it to the list which all gazePoints are in
            allGazePoints.Add(gazePoint);

            //Append by layer (complicated)

            //consider first gazePoint;

            if (gazePointsByLayer.Count == 0) 
            {
                //create new layer of gazePoints in gazeMap
                List<GazePoint> newLayer = new List<GazePoint>();

                //add a new gazePoint to the new layerlist
                newLayer.Add(gazePoint);

                //add the new layer
                gazePointsByLayer.Add(newLayer);
                return;
            }

            //Not the first value
            int queryLayer = gazePoint.Layer;

            int foundLayerIndex = FindLayerIndex(queryLayer);
            if (foundLayerIndex == -1)
            {
                List<GazePoint> newLayer = new List<GazePoint>();
                newLayer.Add(gazePoint);
            }
            else 
            {
                //add at index found
                gazePointsByLayer[foundLayerIndex].Add(gazePoint);
            }

        }

        /// <summary>
        /// Searches gazePointsByLayer till it finds gazePoints that contain the layer supplied with the query layer
        /// </summary>
        /// <param name="queryLayer"></param>
        /// <returns>
        /// returns the index for the sublist of gazepoints if gazePoints of that layer have been found, 
        /// else returns -1;
        /// </returns>
        private int FindLayerIndex(int queryLayer) 
        {
            int foundLayerIndex = 0;
            foreach (List<GazePoint> gazePointLayer in gazePointsByLayer)
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

        //returns the number of gaze points in the gazemap 
        public int getTotalGazeMapSize() 
        {
            return allGazePoints.Count;
        }

        //sets the head gazepoint (used for walking through gaze point map)
        public void setHeadGazePoint(GazePoint newGazePoint) 
        {
            headGazePoint = newGazePoint;
        }
        
        
        /// <summary>
        /// enables or disables all gaze visualisations (and camera path visualisation)
        /// </summary>
        /// <param name="display"></param>
        public void ShowGazeVisualisations(bool display) 
        {
            //Debug.Log("TOGGLING NOW!");
            foreach (GazePoint gp in allGazePoints) 
            {


                GameObject gazeVisualisationSphere = gp.debugSphere;

                //Get the renderer of the sphere, then disable it;
                var gazePointRenderer = gazeVisualisationSphere.GetComponent<Renderer>();

                //toggle
                gazePointRenderer.enabled = display;

                GameObject cameraVisualisationSphere = gp.cameraSphere;
                var cameraPointRenderer = cameraVisualisationSphere.GetComponent<Renderer>();
                bool camCurrentState = cameraPointRenderer.enabled;

                //toggle
                cameraPointRenderer.enabled = display;
            
            
            }
        
        
        }

    }
}