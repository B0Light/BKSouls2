using System.Collections.Generic;
using UnityEngine;

public class RunDirector : MonoBehaviour
{
    public StageGenerator stageGenerator;

    [Header("Stages (4~5)")]
    public List<StageConfig> stages = new List<StageConfig>();

    [Header("Boss Stage")]
    public StageConfig bossStage;

    private int stageIndex = -1;

    [ContextMenu("StartRun")]
    public void StartRun()
    {
        stageIndex = -1;
        NextStage();
    }

    [ContextMenu("NextStage")]
    public void NextStage()
    {
        stageIndex++;

        bool isBoss = stageIndex >= stages.Count;
        var cfg = isBoss ? bossStage : stages[stageIndex];

        if (!stageGenerator || cfg == null)
        {
            Debug.LogError("Missing stageGenerator or StageConfig.");
            return;
        }

        stageGenerator.GenerateStage(cfg, isBoss);
    }
}