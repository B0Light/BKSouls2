using UnityEngine;

public class EntranceTile : RoadTile, IRevenueFacility
{
    public bool AddVisitor(PathFindingUnit visitor)
    {
        visitor.LeaveShelter();
        return true;
    }

    public override void UpdateConnections()
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // 상 (인덱스 0)
            new Vector2Int(-1, 0), // 좌 (인덱스 1)
            new Vector2Int(0, -1), // 하 (인덱스 2)
            new Vector2Int(1, 0)   // 우 (인덱스 3)
        };

        // 연결 정보를 나타내는 4비트 문자열을 담을 배열
        char[] connectionChars = new char[4];

        // 상/하 연결은 무조건 '1'로 설정
        connectionChars[0] = '1'; // 상
        connectionChars[2] = '1'; // 하

        // 좌/우 연결은 주변 도로 존재 여부에 따라 결정
        if (IsRoadAtPosition(originPos + directions[1], directions[1]))
        {
            connectionChars[1] = '1'; // 좌
        }
        else
        {
            connectionChars[1] = '0';
        }

        if (IsRoadAtPosition(originPos + directions[3], directions[3]))
        {
            connectionChars[3] = '1'; // 우
        }
        else
        {
            connectionChars[3] = '0';
        }

        // 배열을 문자열로 변환
        string connectionKey = new string(connectionChars);

        // 연결 상태에 따라 모델 업데이트
        UpdateModel(connectionKey);
    }
}
