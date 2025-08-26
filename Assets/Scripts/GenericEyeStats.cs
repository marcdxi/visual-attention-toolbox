using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///TODO:: DO BY FRAME and TIME not TIME




/// <summary>
/// Class that will calculate "generic" eye information/statistics, 
//  Generic stats are ones that can be calculated without interacting with the environment (e.g blink rate, head direction ect)
/// </summary>
namespace VisualAttentionToolbox
{
    public class GenericEyeStats
    {

        //degrees/s, time
        private List<(float, float)> leftEyeAngularVelocityTime;
        private List<(float, float)> rightEyeAngularVelocityTime;

        //mean of both eyes
        private List<(float, float)> bothEyeAngularVelocityTime;

        //deg/s, time
        private List<(float,float)> headAngularVelocityTime;

        //int (state), time
        private List<(int,float)> blinkTimes;

        public GenericEyeStats() 
        {
            leftEyeAngularVelocityTime = new List<(float, float)>();

            rightEyeAngularVelocityTime = new List<(float, float)>();

            headAngularVelocityTime = new List<(float, float)>();

            blinkTimes = new List<(int, float)>();

            bothEyeAngularVelocityTime = new List<(float, float)>();

        }


        public void UpdateEyeVelocities(float leftEyeVelocity, float rightEyeVelocity, float time) 
        {
            leftEyeAngularVelocityTime.Add((leftEyeVelocity, time));

            rightEyeAngularVelocityTime.Add((rightEyeVelocity, time));

            float meanAngularVelocity = (rightEyeVelocity + leftEyeVelocity) / 2.0f;

            bothEyeAngularVelocityTime.Add((meanAngularVelocity, time));

            
        }


        public void UpdateHeadVelocities(float headAngularVelocity, float time)
        {
            headAngularVelocityTime.Add((headAngularVelocity, time));

        }

    }
}
