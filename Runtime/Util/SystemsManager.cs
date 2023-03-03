using UnityEngine;

namespace SpeedrunSim
{
    public class SystemsManager : PersistentSingleton<SystemsManager>
    {
        
        public void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
