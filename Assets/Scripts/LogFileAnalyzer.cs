using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace VisualAttentionToolbox
{
    public class GazePointAnalyzer : MonoBehaviour
    {
        public string filePath = @"C:\Users\hcila\Documents\ma2586\VisualAttention-ToolboxEnterprise-main - Copy\Assets\StreamingAssets\VATOutputs\Logs05,01,2023,20 48\gazePointLog(0).csv"; // Set this to your CSV file path

        void Start()
        {
            List<Vector3> saccadePoints = ReadAndProcessGazePoints(filePath);
            foreach (Vector3 point in saccadePoints)
            {
                Debug.Log("Saccade Point: " + point);
            }
        }

        List<Vector3> ReadAndProcessGazePoints(string path)
        {
            List<Vector3> saccadePoints = new List<Vector3>();

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                // Skip the header line
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] entries = lines[i].Split(',');
                    if (entries.Length >= 6)
                    {
                        float x, y, z;
                        int fixationId;
                        if (float.TryParse(entries[0], out x) &&
                            float.TryParse(entries[1], out y) &&
                            float.TryParse(entries[2], out z) &&
                            int.TryParse(entries[3], out fixationId))
                        {
                            if (fixationId >= 0) // Assuming non-negative fixationIDs are valid saccade points
                            {
                                saccadePoints.Add(new Vector3(x, y, z));
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("CSV file not found at: " + path);
            }

            return saccadePoints;
        }
    }
}