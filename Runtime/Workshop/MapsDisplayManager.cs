using System;


namespace SpeedrunSim
{
    public class MapsDisplayManager : StaticInstance<MapsDisplayManager>
    {
        public static event Action NewMapsDisplay;
        
        public MapsDisplay officialLevels;
        public MapsDisplay workshopLevels;

        void Start()
        {
            NewMapsDisplay?.Invoke();
        }
    }
}
