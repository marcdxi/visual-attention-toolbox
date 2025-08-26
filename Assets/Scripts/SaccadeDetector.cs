using System.Collections.Generic;
using UnityEngine;

namespace VisualAttentionToolbox
{
    public class SaccadeDetector : MonoBehaviour
    {
        [SerializeField]
        private FixationManager fixationManager;  // Reference to the FixationManager

        [SerializeField]
        private float saccadeThreshold = 0.05f; // Threshold for saccade detection, in seconds

        public List<Saccade> DetectSaccades()
        {
            List<Saccade> saccades = new List<Saccade>();
            if (fixationManager == null || fixationManager.FixationList == null) return saccades;

            var fixations = fixationManager.FixationList;
            for (int i = 0; i < fixations.Count - 1; i++)
            {
                // Calculate the time between the end of one fixation and the start of the next
                float timeBetweenFixations = fixations[i + 1].CentroidTime - (fixations[i].CentroidTime + fixations[i].Duration);

                if (timeBetweenFixations <= saccadeThreshold)
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
                }
            }
            return saccades;
        }
    }
}