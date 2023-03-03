using System;
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.UI;
using TMPro;
using UnityEngine;

namespace SpeedrunSim
{
    public class GhostRunner : StaticInstance<GhostRunner>, ILevelTimeResponder, ILeaderboardEntryInGame
    {
        [SerializeField] LineRenderer path;
        [SerializeField] TMP_Text username;
        [SerializeField] SetUserAvatar avatar;

        Transform _transform;
        LeaderboardEntryRecord _record;
        RunRecord _runRecord;
        Vector3 _startPos;
        Quaternion _startRot;

        Material _ghostRunnerMat;
        [GradientUsage(true)] Gradient _pathGradient;

        public static event Action GhostRunnerLoaded, GhostRunnerUnloaded;
        
        protected override void Awake()
        {
            base.Awake();
            _transform = transform;
            _startPos = _transform.position;
            _startRot = _transform.rotation;
            _pathGradient = new Gradient();
            var colorGradient = path.colorGradient;

            _pathGradient.SetKeys(colorGradient.colorKeys, colorGradient.alphaKeys);
            _ghostRunnerMat = path.material;
            _ghostRunnerMat.SetGradient(_pathGradient);
        }
        
        public void OnRespawnPointReset()
        {
            _transform.SetPositionAndRotation(_startPos, _startRot);
            
        }

        void TimestampsLoaded(RunRecord obj)
        {
            if (_runRecord == null)
            {
                GhostRunnerLoaded?.Invoke();
                //GameTimer.instance.PerFrameTimeResponders.Add(this);
            }

            _runRecord = obj;
            InitLine();
        }

        public void UpdateTime(float ms)
        {
            _transform.position = _runRecord.PosAtTime(ms, out float completionAtTime);
            UpdateLine(completionAtTime);
        }
        
        public void RewindToStamp(int stamp)
        {
            
        }

        public bool BeatTheRunner(int time)
        {
            return _runRecord != null && time < _record.Entry.Score;
        }

        public LeaderboardEntry GetRunnerEntry()
        {
            return _record.Entry;
        }

        public void Display(LeaderboardEntryRecord record)
        {
            if (_record != null)
            {
                _record.ForgetIAsked();
            }
            
            _record = record;
            
            _record.GetTimestamps(TimestampsLoaded);
            
            var user = _record.Entry.User;
            username.SetText(user.Nickname);
            
            avatar.LoadAvatar(user);
            
            TextAlert.instance.Alert($"You are racing {user.Nickname}");
        }

        public void NotifyUpdate(LeaderboardEntryRecord leaderboardEntryRecord)
        {
            if(leaderboardEntryRecord != _record) return;
            _record = null;
            Display(leaderboardEntryRecord);
        }

        void OnDestroy()
        {
            GhostRunnerUnloaded?.Invoke();
        }

        void InitLine()
        {
            var timestamps = _runRecord.RunTimestamps;
            path.positionCount = _runRecord.RunTimestamps.Length;

            for (int i = 0; i < _runRecord.RunTimestamps.Length; i++)
            {
                path.SetPosition(i, timestamps[i]);
            }
            
            UpdateLine(0f);
        }

        void UpdateLine(float completionAtTime)
        {
            var colorKeys = _pathGradient.colorKeys;
            colorKeys[1].time = completionAtTime;
            colorKeys[2].time = completionAtTime;

            _pathGradient.SetKeys(colorKeys, _pathGradient.alphaKeys);
            _ghostRunnerMat.SetGradient(_pathGradient);
        }
    }
}
