using UnityEngine;
using UnityEngine.Animations;

namespace SpeedrunSim
{
    public class FaceCamera : MonoBehaviour
    {
        [SerializeField] LookAtConstraint lookConstraint;

        void Awake()
        {
            var constraint = new ConstraintSource
            {
                sourceTransform = Camera.main.transform,
                weight = 1f
            };

            lookConstraint.AddSource(constraint);
        }
    }
}
