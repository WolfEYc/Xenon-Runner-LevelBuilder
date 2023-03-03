using System;
using UnityEngine;
using UnityEngine.Events;

namespace SpeedrunSim
{
    public class CallableEvent : MonoBehaviour
    {
        [SerializeField] UnityEvent unityEvent;
        public event Action CSharpEvent;

        public void CallEvent()
        {
            unityEvent.Invoke();
            CSharpEvent?.Invoke();
        }
    }
}
