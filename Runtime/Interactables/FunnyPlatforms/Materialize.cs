using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX.Utility;

namespace SpeedrunSim
{
    public class Materialize : MonoBehaviour, INudger
    {
        [SerializeField] [GradientUsage(true)] Gradient materialized, dematerialized;
        [SerializeField] FXResponder fxResponder;
        [SerializeField] UnityEvent onMaterialize, onDematerialize;
        [SerializeField] Timer timer;
        [SerializeField] float duration;
        
        static readonly ExposedProperty Colors = "Colors";

        void Awake()
        {
            timer.Expired += DematerializeObject;
        }

        void Start()
        {
            DematerializeObject();
        }

        public void MaterializeObject()
        {
            onMaterialize.Invoke();
            fxResponder.VFX.SetGradient(Colors, materialized);
            timer.StartCountdown(duration);
        }

        public void DematerializeObject()
        {
            fxResponder.VFX.SetGradient(Colors, dematerialized);
            onDematerialize.Invoke();
        }

        public Vector3 NudgeDir(Vector3 forward)
        {
            return forward;
        }
    }
}
