using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualAttentionToolbox
{
    /// <summary>
    /// Deprecated: Class currently is not used, but intention was class of utility methods for maths
    /// </summary>
    public static class EyeMathUtils 
    {

        //find vector that maps transform A->B
        public static Vector3 getVecAToB(Transform a, Transform b)
        {
            //calculate vector head -> eye
            Vector3 aToB = b.position - a.position;
            
            return aToB;
        }

        //find quaternion that maps Transforms a->b
        //WRONG https://forum.unity.com/threads/get-the-difference-between-two-quaternions-and-add-it-to-another-quaternion.513187/
        //Read forum for correct way to calculate
        //https://stackoverflow.com/questions/22157435/difference-between-the-two-quaternions
        public static Quaternion getRotAToB(Transform from, Transform to) 
        {

            Quaternion toQuat = to.rotation;
            Quaternion fromQuat = from.rotation;

            //quaternion maths, apply the inverse of the second quaternion to get to the eye rotation (usually additive)s
            //there are 

            Quaternion diff = Quaternion.Inverse(fromQuat) * toQuat;
            //Quaternion diff = toQuat * Quaternion.Inverse(fromQuat);

            return diff;

        }

        /// <summary>
        /// Finds angle between two transforms (moves one into seconds location)
        /// </summary>
        /// <param name="from"> original transform </param>
        /// <param name="to"> second transform </param>
        /// <returns>angle betwern rotation quaternions of transorms as float</returns>
        public static float angleBetween(Transform from, Transform to) 
        {
            if (from == null || to == null) { return 0.0f; }

            //rotations of transforms (as rotation)
            Quaternion fromQuat = from.rotation;
            Quaternion toQuat = to.rotation;

            float angleBetween = Quaternion.Angle(fromQuat, toQuat);

            return angleBetween;
        }


    }
}
