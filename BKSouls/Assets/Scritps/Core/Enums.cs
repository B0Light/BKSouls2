using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  USED FOR CHARACTER DATA SAVING
public enum CharacterSlot
{
    CharacterSlot_01,
    CharacterSlot_02,
    CharacterSlot_03,
    CharacterSlot_04,
    CharacterSlot_05,
    NO_SLOT
}

//  USED FOR TO PROCESS DAMAGE, AND CHARACTER TARGETING
public enum CharacterGroup
{
    Team01,
    Team02
}

//  USED TO TAG SLIDERS FOR LEVEL UP UI
public enum CharacterAttribute
{
    Vigor,
    Mind,
    Endurance,
    Strength,
    Dexterity,
    Intelligence,
    Faith
}

public enum BuildUp
{
    Poison,
    Bleed,
    Frost,
}

public enum ItemTier
{
    Common,     //0 white
    Uncommon,   //1 green  
    Rare,       //2 blue
    Epic,       //3 purple
    Legendary,  //4 orange
    Mythic,     //5 Red
    None,       // To out of index
}

public enum ItemType
{
    Weapon,
    Armor,
    Helmet,
    Gauntlet,
    Leggings,
    Consumables,
    Misc,
    Blueprint,
    Spell,
    Potion,
    None,
}

public enum WeaponSlotType
{
    Right,
    Left,
    RightSub,
    LeftSub,
    None
}
//  USED AS A TAG FOR EACH WEAPON MODEL INSTANTIATION SLOT
public enum WeaponModelSlot
{
    RightHand,
    LeftHandWeaponSlot,
    LeftHandShieldSlot,
    BackSlot,
    //Right Hips
    //Left Hips
}

//  USED TO KNOW WHERE TO INSTANTIATE THE WEAPON MODEL BASED ON MODEL TYPE
public enum WeaponModelType
{
    Weapon,
    Shield
}

//  USED FOR ANY INFORMATION SPECIFIC TO A WEAPONS CLASS, SUCH AS BEING ABLE TO RIPOSTE ECT
public enum WeaponClass
{
    StraightSword,
    Spear,
    MediumShield,
    Fist,
    LightShield,
    Bow
}

//  USED TO DETERMINE WHICH ITEM (CATALYST) IS NEEDED TO CAST SPELL
public enum SpellClass
{
    Incantation,
    Sorcery
}

//  USED TO DETERMINE WHICH RANGED WEAPON CAN FIRE THIS AMMO
public enum ProjectileClass
{
    Arrow,
    Bolt
}

public enum ProjectileSlot
{
    Main,
    Secondary
}

//  USED TO TAG EQUIPMENT MODELS WITH SPECIFIC BODY PARTS THAT THEY WILL COVER
public enum EquipmentModelType
{
    FullHelmet,     // WOULD ALWAYS HIDE FACE, HAIR ECT
    Hat,     // WOULD ALWAYS HIDE HAIR
    Hood,           // WOULD ALWAYS HIDE HAIR
    HelmetAcessorie,
    FaceCover,
    Torso,
    Back,
    RightShoulder,
    RightUpperArm,
    RightElbow,
    RightLowerArm,
    RightHand,
    LeftShoulder,
    LeftUpperArm,
    LeftElbow,
    LeftLowerArm,
    LeftHand,
    Hips,
    HipsAttachment,
    RightLeg,
    RightKnee,
    LeftLeg,
    LeftKnee
}

//  USED TO DETERMINE WHICH EQUIPMENT SLOT IS CURRENTLY SELECTED (HELMET, BODY, LEGS, HANDS, RIGHT WEAPON 01, TALISMAN 02, ECT)
public enum EquipmentType
{
    RightWeapon01,  // 0
    RightWeapon02,  // 1
    RightWeapon03,  // 2
    LeftWeapon01,   // 3
    LeftWeapon02,   // 4
    LeftWeapon03,   // 5
    Head,           // 6
    Body,           // 7
    
    MainProjectile, // 8
    SecondaryProjectile, // 9
    QuickSlot01,        // 12
    QuickSlot02,        // 13
    QuickSlot03         // 14
}

//  USED TO TAG HELMET TYPE, SO SPECIFIC HEAD PORTIONS CAN BE HIDDEN DURING EQUIP PROCESS (HAIR, BEARD, ECT)
public enum HeadEquipmentType
{
    FullHelmet, // HIDE ENTIRE HEAD + FEATURES
    Hat,        // DOES NOT HIDE ANYTHING
    Hood,       // HIDES HAIR
    FaceCover   // HIDES BEARD
}

//  USED TO CALCULATE DAMAGE BASED ON ATTACK TYPE
public enum AttackType
{
    LightAttack01,
    LightAttack02,
    HeavyAttack01,
    HeavyAttack02,
    ChargedAttack01,
    ChargedAttack02,
    RunningAttack01,
    RollingAttack01,
    BackstepAttack01,
    LightJumpingAttack01,
    HeavyJumpingAttack01,
    DualAttack01,
    DualAttack02,
    DualJumpAttack,
    DualRunAttack,
    DualRollAttack,
    DualBackstepAttack,
}

//  USED TO CALCULATE DAMAGE ANIMATION INTENSITY
public enum DamageIntensity
{
    Ping,
    Light,
    Medium,
    Heavy,
    Colossal
}

//  USED TO DETERMINE ITEM PICKUP TYPE
public enum ItemPickUpType
{
    WorldSpawn,
    CharacterDrop
}

//  AI STATES
public enum IdleStateMode
{
    Idle,
    Patrol,
    Sleep
        //FOLLOW
        //WANDER
}

public enum BoxOpenType
{
    TopLid,        // 위에 뚜껑이 열리는 것
    SideSliding,   // 컨테이너 박스처럼 양 옆으로 열리는 상자
    SideRotating   // 일반 문처럼 90도 회전하며 열리는 상자
}

public enum LidRotationAxis
{
    X,  // X축 회전 (앞뒤로 열림)
    Y,  // Y축 회전 (좌우로 열림)
    Z   // Z축 회전 (시계/반시계 방향)
}

public enum BoxType
{
    WeaponBox,
    FoodBox,
    SupplyBox,
    MiscBox,
    Safe,
}

public enum Dir
{
    Down,
    Left,
    Up,
    Right,
}

public enum RoomType
{
    Start,
    Battle,
    Event,
    Shop,
    Rest,
    Elite,
    Boss
}

public enum RoomState
{
    None,
    Loading,
    WaitingPlayers,
    Combat,
    Reward,
    Cleared,
    Transition
}