using System;
using System.Collections.Generic;
using UnityEngine;

public class Multimap<TKey, TValue>
{
    private Dictionary<TKey, HashSet<TValue>> keyToValues = new Dictionary<TKey, HashSet<TValue>>();
    private Dictionary<TValue, TKey> valueToKey = new Dictionary<TValue, TKey>();
    
    public void DisplayKeyCounts()
    {
        foreach (var pair in keyToValues)
        {
            TKey key = pair.Key;
            int count = pair.Value.Count; // 연결된 값들의 개수
            Debug.LogWarning($"키: {key}, 값 개수: {count}");
        }
    }

    // 키와 값을 추가하는 메서드
    public bool Add(TKey key, TValue value)
    {
        // 값이 이미 존재하면 추가 불가
        if (valueToKey.ContainsKey(value))
        {
            return false;
        }

        if (!keyToValues.ContainsKey(key))
        {
            keyToValues[key] = new HashSet<TValue>();
        }

        keyToValues[key].Add(value);
        valueToKey[value] = key;
        return true;
    }

    // 키로 값을 가져오는 메서드
    public bool TryGetValuesByKey(TKey key, out IEnumerable<TValue> values)
    {
        if (keyToValues.TryGetValue(key, out HashSet<TValue> valueSet))
        {
            values = valueSet;
            return true;
        }

        values = null;
        return false;
    }

    // 값으로 키를 가져오는 메서드
    public bool TryGetKeyByValue(TValue value, out TKey key)
    {
        return valueToKey.TryGetValue(value, out key);
    }
    
    public int GetValueCountByKey(TKey key)
    {
        // keyToValues에서 값들을 가져옴
        if (keyToValues.TryGetValue(key, out HashSet<TValue> values))
        {
            return values.Count; // 연결된 값의 개수를 반환
        }

        // 키가 존재하지 않으면 0 반환
        return 0;
    }

    public bool RemoveByKey(TKey key)
    {
        // 키가 존재하고 연결된 값이 있을 때
        if (keyToValues.TryGetValue(key, out HashSet<TValue> values) && values.Count > 0)
        {
            // HashSet에서 가장 첫 번째 값 가져오기
            TValue firstValue = default(TValue);
            foreach (var value in values)
            {
                firstValue = value;
                break; // 첫 번째 값만 가져옴
            }

            // 해당 값을 HashSet에서 제거
            if (values.Remove(firstValue))
            {
                // valueToKey에서도 제거
                valueToKey.Remove(firstValue);

                // 만약 해당 키에 연결된 값이 더 이상 없다면, 키도 제거
                if (values.Count == 0)
                {
                    keyToValues.Remove(key);
                }

                return true;
            }
        }

        return false; // 키가 없거나 제거 실패
    }

    // 값으로 항목을 삭제하는 메서드
    public bool RemoveByValue(TValue value)
    {
        if (valueToKey.TryGetValue(value, out TKey key))
        {
            valueToKey.Remove(value);

            if (keyToValues.TryGetValue(key, out HashSet<TValue> valueSet))
            {
                valueSet.Remove(value);

                // 키에 연결된 값이 더 이상 없으면 키 자체 삭제
                if (valueSet.Count == 0)
                {
                    keyToValues.Remove(key);
                }
            }

            return true;
        }
        return false;
    }

    // 모든 데이터를 삭제하는 메서드
    public void Clear()
    {
        keyToValues.Clear();
        valueToKey.Clear();
    }

    // 모든 키를 반환하는 메서드
    public IEnumerable<TKey> GetAllKeys()
    {
        return keyToValues.Keys;
    }

    // 모든 값을 반환하는 메서드
    public IEnumerable<TValue> GetAllValues()
    {
        return valueToKey.Keys;
    }
}