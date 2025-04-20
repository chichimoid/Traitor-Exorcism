using UnityEngine;

namespace ObjectScripts
{
    public class FlashlightUsable : Usable
    {
        private bool _isLight = false;
        [SerializeField] Light _light;

        
        
        protected override void UseFunctional()
        {
            _isLight = !_isLight;
            _light.enabled = _isLight;
        }
        
    }
}