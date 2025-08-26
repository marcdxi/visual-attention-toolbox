using System.Collections;
using UnityEngine;

namespace VisualAttentionToolbox
{
    //based on the quadScript from Heatmap (attribute correctly)/Ask personal tutor
    public class HeatMapScript : MonoBehaviour
    {

        Material mMaterial;
        MeshRenderer mMeshRenderer;

        //
        //mesh points
        //Arranged into x y, (no time dimension currenyly
        float[] mPoints;

        float[] heatMapBuffer;

        ComputeBuffer texCoordsBuffer = null;

        //Number of hits on the mesh
        int mHitCount;

        //zero if should not display, else will display.
        public int overLayHeatMap = 0;

        //delay for hits to fade/come back (not used currently)
        float mDelay;

        int heatMapSize = 1023; //max allowed in unity

        // Use this for initialization
        void Start()
        {
            mDelay = 3;

            mMeshRenderer = GetComponent<MeshRenderer>();
            mMaterial = mMeshRenderer.material;

            mPoints = new float[1023]; //32 point, NEED TO EXTEND THIS (90hz)

            heatMapBuffer = new float[100000];


            texCoordsBuffer = new ComputeBuffer(heatMapBuffer.Length, sizeof(float), ComputeBufferType.Default);

           // secondBuffer = new ComputeBuffer(mPoints.Length, sizeof(float), ComputeBufferType.Default);


        }

        // Update is called once per frame
        void Update()
        {
            //Do nothing (potentially update the heatmap parameters later on)   
        }

        //adds the hitpoint in texture positions (x and y)
        public void AddHitPoint(float xp, float yp)
        {

            
            //Debug.Log("2");

            /*
            mPoints[mHitCount * 3] = xp;
            mPoints[mHitCount * 3 + 1] = yp;

            mHitCount++;
            mHitCount %= heatMapSize;
            */
            //set the float array to the c# array, and set the int for hitCount in the shader
            mMaterial.SetFloatArray("_Hits", mPoints);
            mMaterial.SetInt("_HitCount", mHitCount);

            //mMaterial.SetFloatArray("_HitsX", mPointsX);

            //Alternative method using a compute buffer, 

            float currentTime = Time.time;

            heatMapBuffer[mHitCount * 3] = xp;
            heatMapBuffer[mHitCount * 3 + 1] = yp;
            heatMapBuffer[mHitCount * 3 + 2] = currentTime; //log the current time

            mHitCount++;
            mHitCount %= heatMapBuffer.Length;//prevents overflow


            texCoordsBuffer.SetData(heatMapBuffer);
            mMaterial.SetBuffer("_HitBuffer", texCoordsBuffer);

            //Way of turning the heatmap on or off (Working)
            mMaterial.SetInt("_DisplayHeatMapFlag",overLayHeatMap);

        }

        /// <summary>
        /// Releaases memory from compute buffer when program quits
        /// </summary>
        private void OnApplicationQuit()
        {
            //free memory from compute buffer when application quits.
            texCoordsBuffer.Release();
        }
        /// <summary>
        /// Testing method for the compute buffer
        /// </summary>
        public void ComputeBufferTesting() 
        {
            ComputeBuffer texCoordsBuffer = new ComputeBuffer(mPoints.Length, sizeof(float) , ComputeBufferType.Default);
            texCoordsBuffer.SetData(mPoints);

        }

    }
}