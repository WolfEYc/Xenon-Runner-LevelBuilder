using System.Collections.Generic;
using UnityEngine;

namespace SpeedrunSim
{
    public class TimerMaster : Singleton<TimerMaster>
    {
        readonly List<Timer> _timers = new();
        readonly List<Timer> _swap = new();
        

        /// <summary>
        /// Adds a Timer to be used ECS Style
        /// </summary>
        /// <param name="timer">Timer to be added</param>
        public void AddTimer(Timer timer)
        {
            _timers.Add(timer);
            
            if(_swap.Capacity == _timers.Capacity) return;
            _swap.Capacity = _timers.Capacity;
        }

        public void RemoveTimer(Timer timer)
        {
            _timers.Remove(timer);
        }
        
        void FixedUpdate()
        {
            float time = Time.time;
            _swap.Clear();
            
            for(int i = 0; i < _timers.Count; i++)
            {
                if (_timers[i].endTime > time)
                {
                    _swap.Add(_timers[i]);
                    continue;
                }
                
                _timers[i].Expire();
            }
            
            if(_swap.Count == _timers.Count) return;
            
            _timers.Clear();
            _timers.AddRange(_swap);
            
        }
        
    }
}
