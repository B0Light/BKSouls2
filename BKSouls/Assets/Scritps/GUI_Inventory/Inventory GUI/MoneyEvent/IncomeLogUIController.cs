using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace BK.Inventory
{
    public class IncomeLogUIController : MonoBehaviour
    {
        [SerializeField] private IncomeEventBalance incomeEventChannel;
        [SerializeField] private Transform logContainer;
        [SerializeField] private GameObject logItemPrefab;
        [SerializeField] private int maxLogItems = 10;

        private List<GameObject> logItems = new List<GameObject>();

        private void OnEnable()
        {
            if (incomeEventChannel != null)
            {
                incomeEventChannel.OnIncomeGenerated += HandleIncomeGenerated;
            }
        }

        private void OnDisable()
        {
            if (incomeEventChannel != null)
            {
                incomeEventChannel.OnIncomeGenerated -= HandleIncomeGenerated;
            }
        }

        private void HandleIncomeGenerated(IncomeData incomeData)
        {
            AddLogItem(incomeData);
        }

        private void AddLogItem(IncomeData incomeData)
        {
            // 로그 아이템 생성
            GameObject newLogItem = Instantiate(logItemPrefab, logContainer);
            IncomeLogUI logUI = newLogItem.GetComponent<IncomeLogUI>();

            // 로그 텍스트 설정
            string timeStamp = incomeData.timestamp.ToString("HH:mm:ss");
            string logText = $"[{timeStamp}] {incomeData.attractionName}: +{incomeData.incomeAmount:F0}";
            logUI.Setup(logText);

            // 로그 목록 관리
            logItems.Add(newLogItem);

            // 최대 로그 수 제한
            if (logItems.Count > maxLogItems)
            {
                // 가장 오래된 로그 제거
                GameObject oldestLog = logItems[0];
                logItems.RemoveAt(0);
                Destroy(oldestLog);
            }

            // 5초 후 자동 제거
            StartCoroutine(RemoveLogAfterDelay(newLogItem, 5f));
        }

        private IEnumerator RemoveLogAfterDelay(GameObject logItem, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (logItems.Contains(logItem))
            {
                logItems.Remove(logItem);
                Destroy(logItem);
            }
        }
    }
}