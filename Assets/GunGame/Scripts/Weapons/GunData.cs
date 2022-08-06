using System;

namespace GunGame.Scripts.Weapons
{
    [Serializable]
    public class GunData
    {
        public string GunName;
        public string MagName;


        // Used if weapon pool uses "random within category"
        public int CategoryID;
    }
}
