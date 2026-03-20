using System;

[Serializable]
public class RoomPlan
{
    public RoomType roomType;
    public int templateIndex;
    public int seed;

    public RoomPlan(RoomType roomType, int templateIndex, int seed)
    {
        this.roomType = roomType;
        this.templateIndex = templateIndex;
        this.seed = seed;
    }
}