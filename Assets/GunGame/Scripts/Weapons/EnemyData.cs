using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using FistVR;

namespace GunGame.Scripts.Weapons
{
    [Serializable]
    public class EnemyData
    {
        //Name used for spawning
        public SosigEnemyID EnemyName = 0;
        public String EnemyNameString = "";
        //Value used in various ways, depending on the enemy spawn mode
        public int Value = 0;

        public EnemyData(SosigEnemyID InputName, int InputValue)
        {
            EnemyName = InputName;
            Value = InputValue;
        }

        public EnemyData(String InputNameString, int InputValue)
        {
            EnemyName = (SosigEnemyID)Enum.Parse(typeof(SosigEnemyID), InputNameString, true);
            Value = InputValue;
        }
    }
}
