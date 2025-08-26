using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualAttentionToolbox {
    
    /// <summary>
    /// Manages all areas of interest objects
    /// </summary>
    
    public class AreaOfInterestManager {

        AreaOfInterestScript[] AOIs;

        public AreaOfInterestManager()
        {
            AOIs = FindAllAOIs();
        }

        public AreaOfInterestScript[] FindAllAOIs()
        {
            AreaOfInterestScript[] allAOIs = Object.FindObjectsOfType<AreaOfInterestScript>();
            Debug.Log(("Number of AOIs = ", allAOIs.Length));

            return allAOIs;
        }



        public void GetNumberAOI() 
        {
            Debug.Log(("Number of objects with AOI scripts = "+AOIs.Length));
        } 
    }
}