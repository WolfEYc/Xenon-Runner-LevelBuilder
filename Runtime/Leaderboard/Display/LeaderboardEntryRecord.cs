using System;
using System.Collections.Generic;
using HeathenEngineering.SteamworksIntegration;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpeedrunSim
{
    public class LeaderboardEntryRecord : MonoBehaviour, IDamageable
    {
        public LeaderboardEntry Entry { get; private set; }

        public Object[] inGameViews;
        ILeaderboardEntryInGame[] _inGameViews;
        
        
        RunRecord _timestamps;
        bool _loaded;

        List<Action<RunRecord>> _callbacks;
        

        void Awake()
        {
            _inGameViews = new ILeaderboardEntryInGame[inGameViews.Length];
            for (int i = 0; i < _inGameViews.Length; i++)
            {
                _inGameViews[i] = inGameViews[i] as ILeaderboardEntryInGame;
            }
            
            _callbacks = new List<Action<RunRecord>>();
        }
        
        void LoadTimestamps()
        {
            _loaded = false;

            Entry?.GetAttachedUgc<RunTimestamps>(OnReceivedTimestamps);
        }
        
        void OnReceivedTimestamps(RunTimestamps data, bool failure)
        {
            if (failure)
            {
                Debug.LogError($"failed to load timestamps for {Entry.User.Name}", this);
                return;
            }
            
            _timestamps = new RunRecord(data);
            _loaded = true;

            ProcessCallbacks();
        }

        void ProcessCallbacks()
        {
            foreach (Action<RunRecord> callback in _callbacks)
            {
                callback(_timestamps);
            }
            _callbacks.Clear();
        }
        
        public void SetEntry(LeaderboardEntry entry)
        {
            Entry = entry;
            _loaded = false;
            ForgetIAsked();

            foreach (ILeaderboardEntryInGame inGameView in _inGameViews)
            {
                inGameView.Display(this);
            }
            
            LeaderboardDisplay.instance.GhostRunner.NotifyUpdate(this);
        }

        public void GetTimestamps(Action<RunRecord> callback)
        {
            if(Entry == null) return;
            
            if (_loaded)
            {
                callback(_timestamps);
                return;
            }

            _callbacks.Add(callback);
            LoadTimestamps();
        }

        public void ForgetIAsked()
        {
            _callbacks.Clear();
        }

        public void Damage()
        {
            LeaderboardDisplay.instance.GhostRunner.Display(this);
        }
    }
}
