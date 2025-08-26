using Oculus;

namespace VisualAttentionToolbox 
{
    /// <summary>
    /// Struct that stores all faceExpressions data per Frame, should only be read after writing.
    /// </summary>
    public struct FaceExpressionsFrameData 
    {

        public readonly float[] expressions;

        public readonly float frameTime;

        public readonly int frameCount;

        public readonly float upperFaceConfidence;

        public readonly float lowerFaceConfidence;

        public readonly bool validExpressions;

        public FaceExpressionsFrameData(int passedFrameCount, float passedFrameTime, bool passedValidExpressions, float[] passedExpressions, float passedUpperFaceConfidence, float passedLowerFaceConfidence) 
        {
            frameTime = passedFrameTime;

            frameCount = passedFrameCount;

            expressions = passedExpressions;

            upperFaceConfidence = passedUpperFaceConfidence;

            lowerFaceConfidence = passedLowerFaceConfidence;

            validExpressions = passedValidExpressions;

        }
    
    }



}
