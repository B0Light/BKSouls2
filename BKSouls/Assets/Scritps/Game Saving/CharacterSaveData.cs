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

        [Header("Body Type")]
        public bool isMale = true;
        public int hairStyleID;
        public float hairColorRed;
        public float hairColorGreen;
        public float hairColorBlue;

        [Header("Time Played")]
        public string lastPlayTime; // ISO 8601 형식으로 시간 저장

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
        public int balance;

        [Header("Stats")]
        public int vitality;
        public int mind;
        public int endurance;
        public int strength;
        public int dexterity;
        public int intelligence;
        public int faith;


        [Header("World Items")]
        public SerializableDictionary<int, bool> worldItemsLooted;  //  THE INT IS THE ITEM I.D, THE BOOL IS THE LOOTED STATUS

        [Header("Equipment")]

        public SerializableRangedProjectile mainProjectile;
        public SerializableRangedProjectile secondaryProjectile;

        [Header("Quick Slot Items")]
        public int[] quickSlotItemIDs = new int[3];
        
        public int currentHealthFlasksRemaining = 3;
        public int currentFocusPointsFlaskRemaining = 1;

        [Header("Inventory")]
        public List<SerializableRangedProjectile> projectilesInInventory;
        
        [Header("Inventory Size")] 
        public Vector2Int rightWeaponBoxSize;
        public Vector2Int leftWeaponBoxSize;
        public Vector2Int helmetBoxSize;
        public Vector2Int armorBoxSize;
        public Vector2Int gauntletBoxSize;
        public Vector2Int leggingsBoxSize;
       
    
        public Vector2Int inventoryBoxSize;
        public Vector2Int shareBoxSize;
        public Vector2Int safeBoxSize;
        
        [Header("New Inventory")] 
        public int rightMainWeaponItemCode;
        public int leftMainWeaponItemCode;
        public int rightSubWeaponItemCode;
        public int leftSubWeaponItemCode;
        public int helmetItemCode;
        public int armorItemCode;
        public int gauntletItemCode;
        public int leggingsItemCode;
        
        // key : itemID / value : itemCount
        public SerializableDictionary<int, int> inventoryItems;
        public SerializableDictionary<int, int> backpackItems;
        public SerializableDictionary<int, int> safeItems;
        [Header("ShareInventory")] 
        public SerializableDictionary<int, int> shareInventoryItems;

        //  THIS WILL CHANGE A LITTLE WHEN WE ADD MULTIPLE SPELL SLOTS, IT WILL BE SOMEWHAT SIMILAR TO HOW WEAPONS ARE SAVED
        public int currentSpell;
        
        // Shelter
        public int shelterLevel;
        public List<SaveBuildingData> buildings;

        public CharacterSaveData()
        {
            rightWeaponBoxSize = new Vector2Int(1, 4);
            leftWeaponBoxSize = new Vector2Int(2, 2);
            helmetBoxSize = new Vector2Int(2, 2);
            armorBoxSize = new Vector2Int(2, 2);
            gauntletBoxSize = new Vector2Int(2, 2);
            leggingsBoxSize = new Vector2Int(2, 2);

            inventoryBoxSize = new Vector2Int(6, 3);
            safeBoxSize = new Vector2Int(2, 2);
            shareBoxSize = new Vector2Int(8, 20);
            
            worldItemsLooted = new SerializableDictionary<int, bool>();
            
            projectilesInInventory = new List<SerializableRangedProjectile>();
            
            inventoryItems = new SerializableDictionary<int, int>();
            backpackItems = new SerializableDictionary<int, int>();
            safeItems = new SerializableDictionary<int, int>();
            shareInventoryItems = new SerializableDictionary<int, int>();

            // Shelter
            shelterLevel = 1;
            buildings = new List<SaveBuildingData>();
        }
    }
}
