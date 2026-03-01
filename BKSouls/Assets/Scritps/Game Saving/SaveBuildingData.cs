using System.Collections.Generic;

[System.Serializable]
public class SaveBuildingData
{
    public int x; // 건물의 X 좌표
    public int y; // 건물의 Y 좌표
    public int code; // 건물 코드
    public int dir; // 건물의 회전 방향
    public int level;
    public SaveBuildingData(int x, int y, int code, int dir, int level)
    {
        this.x = x;
        this.y = y;
        this.code = code;
        this.dir = dir;
        this.level = level;
    }
    
    public override bool Equals(object obj)
    {
        // 같은 객체인지 확인
        if (ReferenceEquals(this, obj))
            return true;

        // null 또는 타입이 다른 경우
        if (obj == null || this.GetType() != obj.GetType())
            return false;

        // 각 필드 비교
        SaveBuildingData other = (SaveBuildingData)obj;
        return this.x == other.x && this.y == other.y && this.level == other.level &&
               this.code == other.code && this.dir == other.dir;
    }

    // GetHashCode 메서드 오버라이드
    public override int GetHashCode()
    {
        // 필드를 기반으로 해시 코드 생성
        unchecked // 오버플로우 무시
        {
            int hash = 17;
            hash = hash * 23 + x.GetHashCode();
            hash = hash * 23 + y.GetHashCode();
            hash = hash * 23 + code.GetHashCode();
            hash = hash * 23 + dir.GetHashCode();
            hash = hash * 23 + level.GetHashCode();
            return hash;
        }
    }
}