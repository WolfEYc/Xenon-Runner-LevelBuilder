using UnityEngine;

namespace SpeedrunSim
{
    public interface ISpawnable
    {
         void Instantiate(Vector2 pos, float rot = 0f, float initialForce = 0f);
         
         float sizeRadius { get; }
    }
}
