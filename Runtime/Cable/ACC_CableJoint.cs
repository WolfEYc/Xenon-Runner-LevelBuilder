namespace NOT_Lonely
{
    using UnityEngine;

    public class ACC_CableJoint : MonoBehaviour
    {
        [Tooltip("If checked, this joint object rotation will be updated relative to the cable direction. \nDisable this for a particular use cases, to avoid wrong rotations and rotate this object manually.")]
        public bool autoUpdateRotation = true;
    }
}
