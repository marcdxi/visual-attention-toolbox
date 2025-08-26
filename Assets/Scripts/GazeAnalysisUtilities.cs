using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisualAttentionToolbox
{
    /// <summary>
    /// Provides static utility methods for various calculations and data manipulations related to gaze analysis.
    /// </summary>
    public static class GazeAnalysisUtilities
    {
        private const float SmoothPursuitVelocityThreshold = 20f;
        /// <summary>
        /// Retrieves gaze points between two fixations from a list of all gaze points.
        /// Assumes that Fixation has a List<GazePoint> and GazePoint has a frameCount property.
        /// </summary>
        public static List<GazePoint> GetGazePointsBetween(Fixation start, Fixation end, List<GazePoint> allGazePoints)
        {
            int startIndex = allGazePoints.IndexOf(start.gazePointList[0]);
            int endIndex = allGazePoints.IndexOf(end.gazePointList[0]);
            if (startIndex == -1 || endIndex == -1 || startIndex >= endIndex)
                return new List<GazePoint>();

            return allGazePoints.GetRange(startIndex, endIndex - startIndex);
        }
    /// <summary>
    /// Checks if a set of gaze points represents a smooth pursuit based on velocity and direction consistency.
    /// </summary>
    public static bool IsSmoothPursuit(List<GazePoint> gazePoints)
        {
            if (gazePoints.Count < 2)
                return false;

            for (int i = 1; i < gazePoints.Count; i++)
            {
                Vector3 velocity = (gazePoints[i].WorldSpacePosition - gazePoints[i - 1].WorldSpacePosition) / (gazePoints[i].gazeTime - gazePoints[i - 1].gazeTime);
                Debug.Log("Velocity magnitude: " + velocity.magnitude);

                if (velocity.magnitude > SmoothPursuitVelocityThreshold)
                    return false;

     
            }
            return true;
     //       return CheckVelocityConsistency(gazePoints);
        }

        /// <summary>
        /// Calculates and checks velocity consistency across gaze points.
        /// </summary>
        private static bool CheckVelocityConsistency(List<GazePoint> gazePoints)
        {
            float velocityVarianceThreshold = 10.0f; // Define an appropriate threshold
            var velocities = CalculateVelocities(gazePoints);
        //    float averageVelocity = velocities.Average();

       //     return velocities.All(velocity => Mathf.Abs(velocity - averageVelocity) < velocityVarianceThreshold);

            for (int i = 1; i < velocities.Count; i++)
            {
                if (Mathf.Abs(velocities[i] - velocities[i - 1]) > velocityVarianceThreshold)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calculates velocities between consecutive gaze points.
        /// </summary>
        private static List<float> CalculateVelocities(List<GazePoint> gazePoints)
        {
            List<float> velocities = new List<float>();
            for (int i = 1; i < gazePoints.Count; i++)
            {
                float distance = Vector3.Distance(gazePoints[i - 1].WorldSpacePosition, gazePoints[i].WorldSpacePosition);
                float time = gazePoints[i].gazeTime - gazePoints[i - 1].gazeTime;
                velocities.Add(distance / time);
                Debug.Log("Velocity: " + distance / time);
            }
            return velocities;
        }

        /// <summary>
        /// Checks for directional consistency within gaze points.
        /// </summary>
        private static bool CheckDirectionConsistency(List<GazePoint> gazePoints)
        {
            float directionThreshold = 5.0f; // Degrees, define an appropriate threshold
            var directions = CalculateDirections(gazePoints);
            Vector3 averageDirection = new Vector3(
                directions.Average(dir => dir.x),
                directions.Average(dir => dir.y),
                directions.Average(dir => dir.z));

            return directions.All(dir => Vector3.Angle(dir, averageDirection) < directionThreshold);
        }

        /// <summary>
        /// Calculates directions between consecutive gaze points.
        /// </summary>
        private static List<Vector3> CalculateDirections(List<GazePoint> gazePoints)
        {
            List<Vector3> directions = new List<Vector3>();
            for (int i = 1; i < gazePoints.Count; i++)
            {
                Vector3 direction = (gazePoints[i].WorldSpacePosition - gazePoints[i - 1].WorldSpacePosition).normalized;
                directions.Add(direction);
            }
            return directions;
        }
    }
}