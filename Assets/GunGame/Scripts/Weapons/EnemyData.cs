using JetBrains.Annotations;
using System.Collections.Generic;
using System;
using FistVR;
using UnityEngine;

namespace GunGame.Scripts.Weapons
{
    [Serializable]
    public class EnemyData
    {
        //Name used for spawning
        public SosigEnemyID EnemyName = SosigEnemyID.Misc_Elf;
        public String EnemyNameString = "";
        //Value used in various ways, depending on the enemy spawn mode
        public int Value = 0;

        public EnemyData(SosigEnemyID InputEnum, int InputValue)
        {
            EnemyName = InputEnum;
            Value = InputValue;
        }

        public EnemyData(String InputNameString, int InputValue)
        {
            EnemyName = StringToSosigID(InputNameString);
            Value = InputValue;
        }

        private SosigEnemyID StringToSosigID(String InputString)
        {
            SosigEnemyID sosigID;
            try
            {
                sosigID = (SosigEnemyID)Enum.Parse(typeof(SosigEnemyID), InputString, true);
            }
            catch (Exception _)
            {
                Debug.LogError(InputString + " is not a valid SosigEnemyID, please check your weapon pool format");
                sosigID = SosigEnemyID.Misc_Elf;
            }
            return sosigID;
        }
    }
}
