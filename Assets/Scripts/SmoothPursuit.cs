using System.Collections.Generic;
using UnityEngine;

namespace VisualAttentionToolbox
{
    /// <summary>
    /// Represents a detected smooth pursuit movement between two fixations.
    /// </summary>
    public class SmoothPursuit
    {
        public Fixation StartFixation { get; set; }
        public Fixation EndFixation { get; set; }
        public List<GazePoint> GazePoints { get; private set; }

        public List<Fixation> Fixations { get; private set; }

        private VisualAttentionDataController controller;

        // Constructor to initialize the SmoothPursuit with the relevant fixations and gaze points.
        public SmoothPursuit(Fixation startFixation, Fixation endFixation, List<GazePoint> gazePoints)
        {
            StartFixation = startFixation;
            EndFixation = endFixation;
            GazePoints = gazePoints;
            Fixations = new List<Fixation>();
        }

        private void PopulateFixationList()
        {
            if (controller == null || controller.fixationManager == null) {
                return;
            }

            var fixations = controller.fixationManager.FixationList;
            int startIndex = fixations.IndexOf(StartFixation);
            int EndIndex = fixations.IndexOf(EndFixation);

            if (startIndex == -1 || EndIndex == -1 || startIndex > EndIndex)
                return;

            for (int i = startIndex; i <= EndIndex; i++)
            {
                Fixations.Add(fixations[i]);
            }
        }

        // Method to calculate the average velocity of the gaze points in this smooth pursuit.
        public float CalculateAverageVelocity()
        {
            if (GazePoints == null || GazePoints.Count < 2)
                return 0;

            float totalDistance = 0;
            float totalTime = 0;
            for (int i = 1; i < GazePoints.Count; i++)
            {
                totalDistance += Vector3.Distance(GazePoints[i - 1].WorldSpacePosition, GazePoints[i].WorldSpacePosition);
                totalTime += GazePoints[i].gazeTime - GazePoints[i - 1].gazeTime;
            }
            return totalTime > 0 ? totalDistance / totalTime : 0;
        }

        // Method to calculate the path length of the smooth pursuit.
        public float CalculatePathLength()
        {
            float pathLength = 0;
            for (int i = 1; i < GazePoints.Count; i++)
            {
                pathLength += Vector3.Distance(GazePoints[i - 1].WorldSpacePosition, GazePoints[i].WorldSpacePosition);
            }
            return pathLength;
        }

        // Optionally, include methods to visualize the pursuit or analyze further characteristics.
    }
}