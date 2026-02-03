using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BK
{
    [System.Serializable]
    //  SINCE WE WANT TO REFERENCE THIS DATA FOR EVERY SAVE FILE, THIS SCRIPT IS NOT A MONOBEHAVIOUR AND IS INSTEAD SERIALIZABLE
    public class CharacterSaveData
    {
        [Header("SCENE INDEX")]
        public int sceneIndex = 1;

        [Header("Character Name")]
        public string characterName = "Character";

        [Header("Dead Spot")]
        public bool hasDeadSpot = false;
        public float deadSpotPositionX;
        public float deadSpotPositionY;
        public float deadSpotPositionZ;
        public int deadSpotRuneCount;

        [Header("Body Type")]
        public bool isMale = true;
        public int hairStyleID;
        public float hairColorRed;
        public float hairColorGreen;
        public float hairColorBlue;

        [Header("Time Played")]
        public float secondsPlayed;

        // QUESTION: WHY NOT USE A VECTOR3?
        // ANSWER: WE CAN ONLY SAVE DATA FROM "BASIC" VARIABLE TYPES (Float, Int, String, Bool, ect)
        [Header("World Coordinates")]
        public float xPosition;
        public float yPosition;
        public float zPosition;

        [Header("Resources")]
        public int currentHealth;
        public float currentStamina;
        public int currentFocusPoints;
        public int runes;

        [Header("Stats")]
        public int vitality;
        public int mind;
        public int endurance;
        public int strength;
        public int dexterity;
        public int intelligence;
        public int faith;

        [Header("Sites Of Grace")]
        public int lastSiteOfGraceRestedAt = 0;
        public SerializableDictionary<int, bool> sitesOfGrace;      //  THE INT IS THE SITE OF GRACE I.D, THE BOOL IS THE "ACTIVATED" STATUS

        [Header("Bosses")]
        public SerializableDictionary<int, bool> bossesAwakened;    //  THE INT IS THE BOSS I.D, THE BOOL IS THE AWAKENED STATUS
        public SerializableDictionary<int, bool> bossesDefeated;    //  THE INT IS THE BOSS I.D, THE BOOL IS THE DEFEATED STATUS

        [Header("World Items")]
        public SerializableDictionary<int, bool> worldItemsLooted;  //  THE INT IS THE ITEM I.D, THE BOOL IS THE LOOTED STATUS

        [Header("Equipment")]
        public int headEquipment;
        public int bodyEquipment;
        public int legEquipment;
        public int handEquipment;

        public int rightWeaponIndex;
        public SerializableWeapon rightWeapon01;
        public SerializableWeapon rightWeapon02;
        public SerializableWeapon rightWeapon03;

        public int leftWeaponIndex;
        public SerializableWeapon leftWeapon01;
        public SerializableWeapon leftWeapon02;
        public SerializableWeapon leftWeapon03;

        public int quickSlotIndex;
        public SerilalizableQuickSlotItem quickSlotItem01;
        public SerilalizableQuickSlotItem quickSlotItem02;
        public SerilalizableQuickSlotItem quickSlotItem03;

        public SerializableRangedProjectile mainProjectile;
        public SerializableRangedProjectile secondaryProjectile;

        public int currentHealthFlasksRemaining = 3;
        public int currentFocusPointsFlaskRemaining = 1;

        [Header("Inventory")]
        public List<SerializableWeapon> weaponsInInventory;
        public List<SerializableRangedProjectile> projectilesInInventory;
        public List<SerilalizableQuickSlotItem> quickSlotItemsInInventory;
        public List<int> headEquipmentInInventory;
        public List<int> bodyEquipmentInInventory;
        
        [Header("Inventory Size")] 
        public Vector2Int rightWeaponBoxSize;
        public Vector2Int leftWeaponBoxSize;
        public Vector2Int helmetBoxSize;
        public Vector2Int armorBoxSize;
        public Vector2Int consumableBoxSize;
    
        public Vector2Int inventoryBoxSize;
        public Vector2Int backpackSize;
        public Vector2Int shareBoxSize;
        public Vector2Int safeBoxSize;
        
        [Header("New Inventory")] 
        public int rightWeaponItemCode;
        public int leftWeaponItemCode;
        public int helmetItemCode;
        public int armorItemCode;
        // key : itemID / value : itemCount
        public SerializableDictionary<int, int> quickSlotConsumableItems;
        public SerializableDictionary<int, int> inventoryItems;
        public SerializableDictionary<int, int> backpackItems;
        public SerializableDictionary<int, int> safeItems;
        [Header("ShareInventory")] 
        public SerializableDictionary<int, int> shareInventoryItems;

        //  THIS WILL CHANGE A LITTLE WHEN WE ADD MULTIPLE SPELL SLOTS, IT WILL BE SOMEWHAT SIMILAR TO HOW WEAPONS ARE SAVED
        public int currentSpell;


        public CharacterSaveData()
        {
            rightWeaponBoxSize = new Vector2Int(1, 4);
            leftWeaponBoxSize = new Vector2Int(2, 2);
            helmetBoxSize = new Vector2Int(2, 2);
            armorBoxSize = new Vector2Int(2, 2);
            consumableBoxSize = new Vector2Int(4, 1);

            backpackSize = new Vector2Int(0,0);
            inventoryBoxSize = new Vector2Int(6, 3);
            safeBoxSize = new Vector2Int(2, 2);
            shareBoxSize = new Vector2Int(8, 20);
            
            sitesOfGrace = new SerializableDictionary<int, bool>();
            bossesAwakened = new SerializableDictionary<int, bool>();
            bossesDefeated = new SerializableDictionary<int, bool>();
            worldItemsLooted = new SerializableDictionary<int, bool>();

            weaponsInInventory = new List<SerializableWeapon>();
            projectilesInInventory = new List<SerializableRangedProjectile>();
            quickSlotItemsInInventory = new List<SerilalizableQuickSlotItem>();
            headEquipmentInInventory = new List<int>();
            bodyEquipmentInInventory = new List<int>();
           
        }
    }
}
