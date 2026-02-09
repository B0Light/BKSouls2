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
    PhysicalAttack,     // 0. 물리 공격력 증가
    MagicalAttack,      // 1. 마법 공격력 증가
    PhysicalDefense,    // 2. 물리 방어력 증가
    MagicalDefense,     // 3. 마법 방어력 증가
    HealthPoint,        // 4. 최대 체력 증가
    RestoreHealth,      // 5. 체력 회복
    
    BuffAttack,         // 6. 공격력 버프
    BuffDefense,        // 7. 방어력 버프
    BuffActionPoint,    // 8. 행동력 버프
    UtilitySpeed,       // 9. 이동속도 증가
    UtilityWeight,      // 10. 무게 감소
    Resource,           // 11. 자원 아이템
    StorageSpace,       // 12. 배낭 공간 확장
    None,               // 13. 효과 없음
}
