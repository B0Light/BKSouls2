using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    [System.Serializable]
    public class CharacterClass
    {
        [Header("Class Information")]
        public string className;

        [Header("Class Stats")]
        public int vitality = 10;
        public int endurance = 10;
        public int mind = 10;
        public int strength = 10;
        public int dexterity = 10;
        public int intelligence = 10;
        public int faith = 10;
        //arcane/luck/whatever other stats you want

        [Header("Class Weapons")]
        public WeaponItem mainHandWeapons;
        public WeaponItem offHandWeapons;

        [Header("Class Armor")]
        public HeadEquipmentItem headEquipment;
        public BodyEquipmentItem bodyEquipment;
        public HandEquipmentItem handEquipment;
        public LegEquipmentItem legEquipment;
        
        [Header("Quick Slot Items")]
        public List<QuickSlotItem> quickSlotItems = new List<QuickSlotItem>();

        public void SetClass(PlayerManager player)
        {
            TitleScreenManager.Instance.SetCharacterClass(player, vitality, endurance, mind, strength, dexterity, intelligence, faith,
                mainHandWeapons, offHandWeapons, headEquipment, bodyEquipment, legEquipment, handEquipment, quickSlotItems);
        }

        public void DecideClass(PlayerManager player)
        {
            TitleScreenManager.Instance.DecideCharacterClass(player, vitality, endurance, mind, strength, dexterity, intelligence, faith,
                mainHandWeapons, offHandWeapons, headEquipment, bodyEquipment, legEquipment, handEquipment, quickSlotItems);
        }
    }
}
