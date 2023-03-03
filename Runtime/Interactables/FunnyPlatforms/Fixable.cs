using System;
using UnityEngine;

namespace SpeedrunSim
{
    public class Fixable : MonoBehaviour
    {
        [SerializeField] Transform fractureRoot;
        public bool isCpy;
        public GameObject original;

        public static Action FixAllGlass;
        
        void Awake()
        {
            if(isCpy) return;
            isCpy = true;

            original = gameObject;
            
            Instantiate(this, transform.parent).original = original;
            
            isCpy = false;
        }

        void Start()
        {
            FixAllGlass += Fix;
            
            if(isCpy)
            {
                gameObject.SetActive(false);
            }
        }
        
        
        public void Fix()
        {
            if(original.activeSelf) return;
            
            if (isCpy)
            {
                var cpy = Instantiate(this, transform.parent);
                original = cpy.gameObject;
                cpy.original = original;
                original.SetActive(true);
                cpy.isCpy = false;
                return;
            }

            FixAllGlass -= Fix;
            
            fractureRoot.DestroyChildren();
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            FixAllGlass -= Fix;
        }
    }
}
