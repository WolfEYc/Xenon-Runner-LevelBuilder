#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    [RequireComponent(typeof(UnityEngine.UI.RawImage))]
    public class InputActionGlyph : MonoBehaviour
    {
        public InputActionSet set;
        public InputActionSetLayer layer;
        public InputAction action;

        private UnityEngine.UI.RawImage image;

        private void Start()
        {
            image = GetComponent<UnityEngine.UI.RawImage>();
            RefreshImage();
        }

        private void OnEnable()
        {
            RefreshImage();
        }

        public void RefreshImage()
        {
            if (action != null && image != null)
            {
                if (set != null)
                {
                    var controllers = API.Input.Client.ConnectedControllers;
                    if (controllers.Length > 0)
                    {
                        var textures = action.GetInputGlyphs(controllers[0], set);
                        if (textures.Length > 0)
                        {
                            image.texture = textures[0];
                        }
                    }
                }
                else if (layer != null)
                {
                    var controllers = API.Input.Client.ConnectedControllers;
                    if (controllers.Length > 0)
                    {
                        var textures = action.GetInputGlyphs(controllers[0], layer);
                        if (textures.Length > 0)
                        {
                            image.texture = textures[0];
                        }
                    }
                }
            }
        }
    }
}
#endif