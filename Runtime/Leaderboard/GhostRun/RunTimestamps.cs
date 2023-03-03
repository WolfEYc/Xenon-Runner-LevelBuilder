using System;
using UnityEngine;


namespace SpeedrunSim
{
    [Serializable]
    public struct RunTimestamps
    {
        public Vector4[] timeStamps;

        public RunTimestamps(Vector4[] timeStamps)
        {
            this.timeStamps = timeStamps;
        }
    }

    public class RunRecord
    {
        public readonly Vector4[] RunTimestamps;
        int _currentTimestamp;
        readonly float[] _runCompletionSlices;
        
        public RunRecord(RunTimestamps timestamps)
        {
            RunTimestamps = timestamps.timeStamps;
            _currentTimestamp = 0;

            _runCompletionSlices = new float[RunTimestamps.Length];

            //convert for distance
            _runCompletionSlices[0] = 0f; 
            
            for (int i = 1; i < _runCompletionSlices.Length; i++)
            {
                _runCompletionSlices[i] =
                    _runCompletionSlices[i - 1] + 
                    Vector3.Distance(RunTimestamps[i - 1], RunTimestamps[i]);
            }

            //convert for percent
            for (int i = 1; i < _runCompletionSlices.Length; i++)
            {
                _runCompletionSlices[i] /= _runCompletionSlices[^1];
            }
        }

        float CompletionAtIndex(int idx, float lerpToNext)
        {
            return Mathf.Lerp(_runCompletionSlices[idx], _runCompletionSlices[idx + 1], lerpToNext);
        }

        public Vector3 PosAtTime(float ms, out float completionAtTime)
        {
            while (_currentTimestamp > 0 &&
                ms < RunTimestamps[_currentTimestamp].w)
            {
                _currentTimestamp--;
            }
            
            while (_currentTimestamp + 1 < RunTimestamps.Length - 1 
                   && ms > RunTimestamps[_currentTimestamp + 1].w)
            {
                _currentTimestamp++;
            }

            Vector4 currentTimestamp = RunTimestamps[_currentTimestamp];
            Vector4 nextTimeStamp = RunTimestamps[_currentTimestamp + 1];
            
            if (ms > nextTimeStamp.w)
            {
                completionAtTime = 1f;
                return nextTimeStamp;
            }

            float lerp = ms.Remap(currentTimestamp.w, nextTimeStamp.w, 0f, 1f);

            completionAtTime = CompletionAtIndex(_currentTimestamp, lerp);
            
            return Vector3.Lerp(
                currentTimestamp,
                nextTimeStamp,
                    lerp
                );
        }
        
        
    }
}
