using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace VisualAttentionToolbox
{
    /// <summary>
    /// Superclass for fixation identifiers (Implement after fixation aggregation done)
    /// </summary>
    public class FixationIdentifier
    {
        //determines if fixationIdentifier should use world (True) or local (false) Positions
        public bool useWorldPositions = true;

        //generically checks if two gazePoints are within the same fixation
        public virtual bool DetermineFixation(GazePoint prev, GazePoint next)
        {
            throw new System.InvalidOperationException("Running Virtual DetermineFixation function!!!" +
                                                        "\n Cannot fixation without specified identifier");

        }

        /// <summary>
        /// determines if two gazePoints should be in the fixation based on their angular velocity
        /// </summary>
        /// <param name="prev">gazePoint from previous frame</param>
        /// <param name="next">gazePoint from the current frame</param>
        /// <param name="incomingAngularVelocity">incoming angular velocity in deg/s</param>
        /// <returns></returns>
        public virtual bool DetermineFixation(GazePoint prev, GazePoint next, float incomingAngularVelocity)
        {
            throw new System.InvalidOperationException("Running Virtual DetermineFixation function!!!" +
                                                        "\n Cannot fixation without specified identifier");

        }

    }
}
