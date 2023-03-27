using JetBrains.Annotations;
using System.Collections.Generic;
using System;

namespace GunGame.Scripts.Weapons
{
    [Serializable]
    public class GunData
    {
        public string GunName;
        public string MagName;
        //Extra variable for multiple mag names
        public List<string> MagNames;
        public string Extra;

        // Used if weapon pool uses "random within category"
        public int CategoryID;

        public override String ToString()
        {
            return ("Name: " + GunName + ", MagName: " + MagName + ", Extra: " + Extra);
        }
    }
}
