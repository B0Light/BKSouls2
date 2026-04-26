using UnityEngine.Serialization;

[System.Serializable]
public class ItemAbility
{
    public ItemEffect itemEffect;
    public int value;
    
    public ItemAbility(ItemEffect itemEffect, int value)
    {
        this.itemEffect = itemEffect;
        this.value = value;
    }
}

public enum ItemEffect
{
    PhysicalAttack,     // 1. 물리 공격력 증가
    MagicalAttack,      // 2. 마법 공격력 증가
    PhysicalDefense,    // 3. 물리 방어력 증가
    MagicalDefense,     // 4. 마법 방어력 증가
    RestoreHealth,      // 5. 체력 회복
    RestoreMana,        // 6. 마나 회복
    FireDamage,        // 7. 화염 피해
    IceDamage,         // 8. 냉기 피해
    LightningDamage,   // 9. 번개 피해
    BleedDamage,        // 10. 출혈 피해
    PoisonDamage,      // 11. 독 피해
    Resource,           // 98. 자원
    None,               // 99. 효과 없음
}