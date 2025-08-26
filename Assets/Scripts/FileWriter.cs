using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

namespace VisualAttentionToolbox{ 
    
    /// <summary>
    /// Class of utility functions for writing to files, currently only gazePoints (raw gaze data) fixations and face expressions are stored.
    /// </summary>
    
    public class FileWriter 
    {
        //[SerializeField]
        private string logFileDirectory;

        private string logFolderDir = Application.streamingAssetsPath + " /VATOutputs/";
        
        private string otherDir;
        public FileWriter() 
        {

            
            //create a directory of VisualAttentionOutputs

            bool mainFolderExists = Directory.Exists(logFolderDir);

            if (!mainFolderExists)
            {
                Directory.CreateDirectory(logFolderDir);
            }

            //main folder now exists, now make the directory for the subfiles
            DateTime currentTime = DateTime.Now;

            string formattedStringTime = currentTime.ToString("MM,dd,yyyy,HH mm");
            Debug.Log(formattedStringTime);


            
            logFileDirectory = logFolderDir + "Logs" + formattedStringTime + "/";
            Debug.Log("SubFolderDir = ");
            Debug.Log(logFileDirectory);
            if (!Directory.Exists(logFileDirectory))
            {
                Directory.CreateDirectory(logFileDirectory);
            }




        }

        public void WriteAllGazePointsToFile(VisualAttentionDataController vadc) 
        {

            int totalNumGazePoints = vadc.gazeMap.getTotalGazeMapSize();

            StringBuilder sb = new StringBuilder("", 500); //can estimate based on totalNumgazePoints..... (faster?)
            
            sb.Append("FrameCount,frameTime,Global Position X,Global Position Y,Global Position Z,fixationID,LayerNumber,Hit object name\n");

            for (int i = 0; i < totalNumGazePoints; i++)
            {
                GazePoint gp = vadc.gazeMap.AllGazePoints(i);
                //global positionx, global position y, global position z, fixationID, Time,
                //For now
                sb.Append(gp.frameCount);
                sb.Append(",");
                sb.Append(gp.gazeTime);
                sb.Append(",");
                sb.Append(gp.WorldSpacePosition.x);
                sb.Append(",");
                sb.Append(gp.WorldSpacePosition.y);
                sb.Append(",");
                sb.Append(gp.WorldSpacePosition.z);
                sb.Append(",");
                sb.Append(gp.fixationID);
                sb.Append(",");
                sb.Append(gp.Layer);
                sb.Append(",");
                sb.Append(gp.GazeRayHit.collider.gameObject.name);
                sb.Append("\n");

            }
            //for making sure the file exists


            //COULD USE DO, WHILE???
            int gazePointLogNumber = 0;
            string newFilePath = logFileDirectory + "/gazePointLog("+gazePointLogNumber+").csv";

            //loop until a file with new name exists
            while (File.Exists(newFilePath)) 
            {
                gazePointLogNumber++;
                newFilePath = logFileDirectory + "/gazePointLog(" + gazePointLogNumber + ").csv";

            }
            
            FindUseableFilePath("gazePointLog");

            //write to the new file
            File.WriteAllText(newFilePath, sb.ToString());
            Debug.Log("\n\n\n\nFile Written at path "+ newFilePath+ "\n\n\n\n");
        }

        /// <summary>
        /// Iterates through filePaths with extension (number) until either one is found, or 
        /// </summary>
        /// <param name="desiredFileName"> desired filename, will always be given the logFileDirectory file directory</param>
        /// <returns></returns>
        private string FindUseableFilePath(string desiredFileName) 
        {

            int fileNumber = 0;


            string newFilePath = logFileDirectory + "/" + desiredFileName + ".csv";

            //iterate through filenames with 
            while (File.Exists(newFilePath))
            {
                fileNumber++;
                newFilePath = logFileDirectory + "/" + desiredFileName + "(" + fileNumber + ")" + ".csv";

                if (fileNumber >= 100) 
                {
                    throw new System.Exception("over 100 attempts have been made to write with the desired filename , file has not been saved");
                } 

            }

            return newFilePath;

        }


        public void LogFixationsToFile(VisualAttentionDataController vadc) 
        {

            StringBuilder sb = new StringBuilder("", 500); //can estimate based on totalNumgazePoints..... (faster?)

            sb.Append("fixation number, world centroid position x, world centroid position y , world centroid position z, centroid time, fixation duration\n");

            List<Fixation> fixations = vadc.fixationManager.FixationList;

            foreach (Fixation fixation in fixations) 
            {
                sb.Append(fixation.ID);
                sb.Append(",");
                sb.Append(fixation.CentroidPosition.x);
                sb.Append(",");
                sb.Append(fixation.CentroidPosition.y);
                sb.Append(",");
                sb.Append(fixation.CentroidPosition.z);
                sb.Append(",");
                sb.Append(fixation.CentroidTime);
                sb.Append(",");
                sb.Append(fixation.Duration);
                sb.Append("\n");
            }

            string usedFilePath = FindUseableFilePath("fixationLogFile");

            File.WriteAllText(usedFilePath, sb.ToString());

        }

        /// <summary>
        /// Implement generic class first
        /// </summary>
        public void LogAngularVelocitiesToFile() 
        {
            
            
            
        }

        
        public void LogExpressionFrameDataToFile(VisualAttentionDataController vadc) 
        {

            StringBuilder sb = new StringBuilder("", 500);
            sb.Append("FrameCount, FrameTime, validExpressions,");
            //This line is stupidly long
            sb.Append("BrowLowererL, BrowLowererR, CheekPuffL, CheekPuffR, CheekRaiserL, CheekRaiserR, CheekSuckL, CheekSuckR, ChinRaiserB, ChinRaiserT, DimplerL, DimplerR, EyesClosedL, EyesClosedR, EyesLookDownL, EyesLookDownR, EyesLookLeftL, EyesLookLeftR, EyesLookRightL, EyesLookRightR, EyesLookUpL, EyesLookUpR, InnerBrowRaiserL, InnerBrowRaiserR, JawDrop, JawSidewaysLeft, JawSidewaysRight, JawThrust, LidTightenerL, LidTightenerR, LipCornerDepressorL, LipCornerDepressorR, LipCornerPullerL, LipCornerPullerR, LipFunnelerLB, LipFunnelerLT, LipFunnelerRB, LipFunnelerRT, LipPressorL, LipPressorR, LipPuckerL, LipPuckerR, LipStretcherL, LipStretcherR, LipSuckLB, LipSuckLT, LipSuckRB, LipSuckRT, LipTightenerL, LipTightenerR, LipsToward, LowerLipDepressorL, LowerLipDepressorR, MouthLeft, MouthRight, NoseWrinklerL, NoseWrinklerR, OuterBrowRaiserL, OuterBrowRaiserR, UpperLidRaiserL, UpperLidRaiserR, UpperLipRaiserL, UpperLipRaiserR, Max,");
            sb.Append("UpperFaceConfidence, LowerFaceConfidence\n");

            foreach (FaceExpressionsFrameData fd in vadc.expressionsFrameData) 
            {
                sb.Append(fd.frameCount);
                sb.Append(',');
                sb.Append(fd.frameTime);
                sb.Append(',');
                sb.Append(fd.validExpressions.ToString() + ',');
                sb.Append(',');

                foreach (float f in fd.expressions) 
                {
                    sb.Append(f.ToString());
                    sb.Append(',');
                }

                sb.Append(fd.upperFaceConfidence);
                sb.Append(',');
                sb.Append(fd.lowerFaceConfidence);
                sb.Append('\n');
            
            }

            string usedFilePath = FindUseableFilePath("faceExpressionsFrameDataLog");

            File.WriteAllText(usedFilePath, sb.ToString());


        }

        public void LogVisualAttentionFrameDataToFile(VisualAttentionDataController vadc) 
        {
            StringBuilder sb = new StringBuilder("", 500); //can estimate based on totalNumgazePoints..... (faster?)

            //sb.Append("fixation number, world centroid position x, world centroid position y , world centroid position z, centroid time, fixation duration, fixation size");

            //sb.Append("frameCount, left eye world position x, left eye world position y, left eye world position z, left eye time, " +
            //                    "right eye world position x, right eye world position y, right eye world position z, right eye time");

            //LEFT EYE GAZE POINT METRICS
            sb.Append("frameCount, "); //1
            //left eye gaze point information
            sb.Append("left Gaze point world position x, left Gaze point world position y, left Gaze point world position z, left Gaze point gazeTime, left Gaze point hit layer, leftEyeHitObject,"); //7
            //right eye gaze point information
            sb.Append("right Gaze point world position x, right Gaze point world position y, right Gaze point world position z, right Gaze point gazeTime, right Gaze point hit layer, rightEyeHitObject,");//13
            //other metrics
            sb.Append("left eye open confidence, left eye position x, left eye position y, left eye position z, right eye open confidence, right eye position x, right eye position y, right eye position z, "); //21
            //not doing conjugate or disconjugate
            sb.Append("isConjugate, isFixational,"); //23
            //camera position
            sb.Append("headCameraPosition.x, headCameraPosition.y, headCameraPosition.z,"); //26
            //head/eye speeds
            sb.Append("head angular speed, left eye angular speed, right eye angular speed"); //29
            sb.Append("\n");

            List <VisualAttentionFrameData> frameData = vadc.VATFrameData;

            foreach (VisualAttentionFrameData frame in frameData) 
            {
                sb.Append(frame.frameCount);
                sb.Append(",");
                if (frame.leftEyeGazePoint != null)
                {
                    sb.Append(frame.leftEyeGazePoint.WorldSpacePosition.x);
                    sb.Append(",");
                    sb.Append(frame.leftEyeGazePoint.WorldSpacePosition.y);
                    sb.Append(",");
                    sb.Append(frame.leftEyeGazePoint.WorldSpacePosition.z);
                    sb.Append(",");
                    sb.Append(frame.leftEyeGazePoint.gazeTime);
                    sb.Append(",");
                    sb.Append(frame.leftEyeGazePoint.Layer);
                    sb.Append(",");
                    sb.Append(frame.leftEyeGazePoint.GazeRayHit.collider.name);
                    sb.Append(",");
                    //7
                }
                else
                {
                    sb.Append("null");
                    sb.Append(",");
                    sb.Append("null");
                    sb.Append(",");
                    sb.Append("null");
                    sb.Append(",");
                    sb.Append("null");
                    sb.Append(",");
                    sb.Append("null");
                    sb.Append(",");
                    sb.Append("null");
                    sb.Append(",");

                }

                if (frame.rightEyeGazePoint != null)
                {
                    sb.Append(frame.rightEyeGazePoint.WorldSpacePosition.x);
                    sb.Append(",");
                    sb.Append(frame.rightEyeGazePoint.WorldSpacePosition.y);
                    sb.Append(",");
                    sb.Append(frame.rightEyeGazePoint.WorldSpacePosition.z);
                    sb.Append(",");
                    sb.Append(frame.rightEyeGazePoint.gazeTime);
                    sb.Append(",");
                    sb.Append(frame.rightEyeGazePoint.Layer);
                    sb.Append(",");
                    sb.Append(frame.rightEyeGazePoint.GazeRayHit.collider.name);
                    sb.Append(",");
                    //13
                }
                else
                {
                    sb.Append("null");
                    sb.Append(",");
                    sb.Append("null");
                    sb.Append(",");
                    sb.Append("null");
                    sb.Append(",");
                    sb.Append("null");
                    sb.Append(",");
                    sb.Append("null");
                    sb.Append(",");
                    sb.Append("null");
                    sb.Append(",");
                }
                //other eye metrics
                sb.Append(frame.leftEyeOpen);
                sb.Append(",");
                sb.Append(frame.leftEyePosition.x);
                sb.Append(",");
                sb.Append(frame.leftEyePosition.y);
                sb.Append(",");
                sb.Append(frame.leftEyePosition.z);
                sb.Append(",");
                sb.Append(frame.rightEyeOpen);
                sb.Append(",");
                sb.Append(frame.rightEyePosition.x);
                sb.Append(",");
                sb.Append(frame.rightEyePosition.y);
                sb.Append(",");
                sb.Append(frame.rightEyePosition.z);
                sb.Append(",");
                sb.Append(frame.isConjugate);
                sb.Append(",");
                sb.Append(frame.isFixational);
                sb.Append(",");
                sb.Append(frame.headCameraPosition.x);
                sb.Append(",");
                sb.Append(frame.headCameraPosition.y);
                sb.Append(",");
                sb.Append(frame.headCameraPosition.z);
                sb.Append(",");
                sb.Append(frame.incomingHeadAngularVelocity);
                sb.Append(",");
                sb.Append(frame.incomingLeftEyeAngularVelocity);
                sb.Append(",");
                sb.Append(frame.incomingRightEyeAngularVelocity);
                sb.Append("\n");
                //29
            }

            string usedFilePath = FindUseableFilePath("VisualAttentionFrameDataLog");

            File.WriteAllText(usedFilePath, sb.ToString());

        }

        public void LogAOIInfoToFile(VisualAttentionDataController vadc) 
        {
            AreaOfInterestManager AOIManager = vadc.AOIManager;

            //find all AOIs
            AreaOfInterestScript[] allAOIs = AOIManager.FindAllAOIs();

            List<GameObject> allObjs = new List<GameObject>();

            StringBuilder sb = new StringBuilder("", 500);
            sb.Append("ObjectName, first found frame Count, firstFoundTime, Cumulative Gaze Duration, numberOfGazes, number of gaze points, mean gaze duration\n");

            foreach (AreaOfInterestScript AOI in allAOIs) 
            {
                sb.Append(AOI.gameObject.name);
                sb.Append(",");
                sb.Append(AOI.firstFoundFrameCount);
                sb.Append(",");
                sb.Append(AOI.firstFoundTime);
                sb.Append(",");
                sb.Append(AOI.CumulativeGazeDuration);
                sb.Append(",");
                sb.Append(AOI.numberOfGazes);
                sb.Append(",");
                sb.Append(AOI.ObjectGazePoints.Count);
                sb.Append(",");
                sb.Append(AOI.avgGazeDuration);
                sb.Append("\n");
            }

            string usedFilePath = FindUseableFilePath("AOI Metrics");

            File.WriteAllText(usedFilePath, sb.ToString());

        }
        
    }
}
