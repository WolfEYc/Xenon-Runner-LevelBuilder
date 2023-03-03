using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SpeedrunSim
{
    public static class ExtensionMethods
    {
        public const int PLAYER_ATTACKS_LAYER = 14;
        public const int PLAYER_LAYER = 6;

        public const int ENEMY_ATTACKS_LAYER = 18;
        public const int ENEMY_LAYER = 11;

        public static readonly LayerMask PlayerMask = Physics2D.GetLayerCollisionMask(ENEMY_LAYER), EnemyMask = Physics2D.GetLayerCollisionMask(PLAYER_LAYER);
        public static WaitForSecondsRealtime SlowPollUpdate;
        
        public const float TWO_PI = 2f * Mathf.PI;
        const float k_TwoThirdsPi = TWO_PI / 3f;
        const float k_FourThirdsPi = 2f * k_TwoThirdsPi;

        static RaycastHit[] _sphereCastHits;
        static readonly int GradientTex = Shader.PropertyToID("_GradientTex");
        static Texture2D _gradientTexture;
        public static Rigidbody PlayerRb;
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            _sphereCastHits = new RaycastHit[69];
            _gradientTexture = new(256, 1);
            SlowPollUpdate = new WaitForSecondsRealtime(1);
        }

        public static bool AmPlayerAttack(this Component component)
        {
            return component.gameObject.layer == PLAYER_ATTACKS_LAYER;
        }

        public static bool AmEnemyAttack(this Component component)
        {
            return component.gameObject.layer == ENEMY_ATTACKS_LAYER;
        }

        public static bool AmPlayer(this Component component)
        {
            return component.gameObject.layer is PLAYER_LAYER;
        }

        public static void DestroyChildren(this Transform t)
        {
            foreach (Transform child in t)
            {
                Object.Destroy(child.gameObject);
            }
        }

        public static void AddLayer(this ref LayerMask originalLayerMask, int layerToAdd)
        {
            originalLayerMask |= (1 << layerToAdd);
        }

        public static void RemoveLayer(this ref LayerMask originalLayerMask, int layerToRemove)
        {
            originalLayerMask &= ~(1 << layerToRemove);
        }

        public static bool InLayer(this ref LayerMask layerMask, int layer)
        {
            return layerMask == (layerMask | (1 << layer));
        }

        public static Vector2 DirectionFromAngleDegrees(this float angle)
        {
            return DirectionFromAngle(angle * Mathf.Deg2Rad);
        }

        public static void Shuffle<T>(this T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = Random.Range(0, n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
        }

        public static T RandomElement<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static Vector2 DirectionFromAngle(this float angle)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        public static Vector2 PosInCircle(float radius, int index, int count)
        {
            float radians = (float)index / count * TWO_PI;
            return new Vector2(radius * Mathf.Cos(radians), radius * Mathf.Sin(radians));
        }

        public static float AngleFromDirection(this Vector2 pos)
        {
            pos.Normalize();
            return Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
        }

        public static void OrientAlongVelocity(this Rigidbody2D rb)
        {
            if (rb.velocity == Vector2.zero) return;
            rb.rotation = rb.velocity.AngleFromDirection();
        }
        
        public static void OrientAlongVelocity(this Rigidbody rb)
        {
            if (rb.velocity == Vector3.zero) return;
            rb.rotation = Quaternion.LookRotation(rb.velocity);
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static Color Complementary(this Color color)
        {
            Color returned = Color.white - color;
            returned.a = color.a;
            return returned;
        }

        public static float Intensity(this Color color)
        {
            return (color.r + color.b + color.g) / 3f;
        }

        public static Color rainbow {
            get
            {
                float time = Time.time;

                return new Color(
                    Mathf.Sin(time).Remap(-1, 1, 0, 1),
                    Mathf.Sin(time + k_TwoThirdsPi).Remap(-1, 1, 0, 1),
                    Mathf.Sin(time + k_FourThirdsPi).Remap(-1, 1, 0, 1)
                );
            }
        }
        
        public static Color SetRGB(this Color color, Color replacement)
        {
            return new Color(replacement.r, replacement.g, replacement.b, color.a);
        }

        public static Quaternion RotationFromDirection2D(this Vector2 direction)
        {
            return Quaternion.AngleAxis(direction.AngleFromDirection(), Vector3.forward);
        }

        public static Quaternion RotationFromAngle2D(this float angle)
        {
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        const float k_Dmgmin = 0f, k_Dmgmax = 100f;
        const float k_Scalemin = 0.5f, k_Scalemax = 4f;
        
        public static float CalculateFXScale(this float resultDmg)
        {
            return resultDmg.Remap(k_Dmgmin, k_Dmgmax, k_Scalemin, k_Scalemax);
        }

        public static bool IsVisible(this Camera camera, Vector3 point)
        {
            Vector3 viewportPoint = camera.WorldToViewportPoint(point);

            return viewportPoint.x is > 0f and < 1f
                   && viewportPoint.y is > 0f and < 1f
                   && viewportPoint.z > 0f;
        }

        public static bool LookingAwayFrom(this Transform transform, Vector3 point)
        {
            return Vector3.Dot(transform.forward, Vector3.Normalize(point - transform.position)) < 0f;
        }
        
        /// <summary>
        /// Returns the first hit in a cone
        /// </summary>
        /// <param name="origin">tip of cone</param>
        /// <param name="coneAngle">how wide our cone be in degrees</param>
        /// <param name="maxRadius">radius of spherecast</param>
        /// <param name="direction">which way cone shoot</param>
        /// <param name="maxDistance">how long cone is</param>
        /// <param name="layerMask">what cone hits</param>
        /// <param name="hit">closest hit in cone</param>
        /// <returns>Whether cone hit or not</returns>
        public static bool ConeCast(Vector3 origin, float coneAngle, float maxRadius, Vector3 direction, float maxDistance, LayerMask layerMask, out RaycastHit hit)
        {
            var size = Physics.SphereCastNonAlloc(origin, maxRadius, direction, _sphereCastHits, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
            hit = default;
            
            if (size <= 0) return false;

            float lowestSqrMag = float.PositiveInfinity;
            
            for (int i = 0; i < size; i++)
            {
                Vector3 hitPoint = _sphereCastHits[i].point;
                Vector3 directionToHit = hitPoint - origin;
                float angleToHit = Vector3.Angle(direction, directionToHit);

                float sqrMag = directionToHit.sqrMagnitude;

                if (angleToHit > coneAngle || sqrMag > lowestSqrMag) continue;
                
                hit = _sphereCastHits[i];
                lowestSqrMag = sqrMag;
            }

            return lowestSqrMag < float.PositiveInfinity;
        }

        public static Vector2 RandomDirection()
        {
            return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }

        public static Vector3 PosAtFOV(this Camera camera, Vector3 origin, float desiredFOV)
        {
            float currentFOV = camera.fieldOfView;
            camera.fieldOfView = desiredFOV;
            Vector3 viewPointAtDesired = camera.WorldToViewportPoint(origin);
            camera.fieldOfView = currentFOV;
            return camera.ViewportToWorldPoint(viewPointAtDesired);
        }

        public static string TimeStringFromMS(this float ms)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(ms);
            return $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
        }
        
        public static string TimeStringFromMS(this int ms)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(ms);
            return $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
        }

        public static void SetGradient(this Material material, Gradient gradient)
        {
            for (int i = 0; i < 256; i++)
            {
                float t = i / 255f;
                Color color = gradient.Evaluate(t);
                _gradientTexture.SetPixel(i, 0, color);
            }
            _gradientTexture.Apply();

            // Set the material's "_GradientTex" property to the new Texture2D object
            material.SetTexture(GradientTex, _gradientTexture);
        }

        public static int ToPercent(this float percent)
        {
            return (int)(percent * 100f);
        }
    }
}
