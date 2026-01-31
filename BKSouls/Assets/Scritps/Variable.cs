using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Variable<T> 
{
    [SerializeField] private T _value; 
    
     public event Action<T> OnValueChanged;

    public Variable(T initialValue)
    {
        _value = initialValue;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
    }
    
    public void ClearAllSubscribers()
    {
        OnValueChanged = null;
    }
}

[Serializable]
public class ClampedVariable<T> where T : IComparable<T>
{
    [SerializeField] private T _value;
    [SerializeField] private T _maxValue;
    [SerializeField] private T _minValue;

    public event Action<T> OnSetMaxValue;
    public event Action<T> OnValueChanged;
    public event Action<T> OnDepleted;

    public ClampedVariable(T initialValue, T minValue, T maxValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        _value = Clamp(initialValue, _minValue, _maxValue);
    }

    public ClampedVariable(T initialValue) : this(initialValue, default(T), initialValue) { }

    public T Value
    {
        get => _value;
        set
        {
            var clampedValue = Clamp(value, _minValue, _maxValue);
            if (!EqualityComparer<T>.Default.Equals(_value, clampedValue))
            {
                _value = clampedValue;
                OnValueChanged?.Invoke(_value);

                if (_value.CompareTo(_minValue) == 0)
                {
                    OnDepleted?.Invoke(_value);
                }
            }
        }
    }

    public T MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            _value = _maxValue;
            OnSetMaxValue?.Invoke(_maxValue);
        }
    }

    private T Clamp(T value, T min, T max)
    {
        if (value.CompareTo(min) < 0) return min;
        if (value.CompareTo(max) > 0) return max;
        return value;
    }
}



[Serializable]
public class VariableList<T>
{
    [SerializeField] private List<T> _value = new List<T>();

    public event Action<List<T>> OnListChanged;
    public event Action<T> OnItemAdded;
    public event Action<T> OnItemRemoved;
    public event Action OnListCleared;

    public List<T> Value
    {
        get => _value;
        set
        {
            _value = value;
            OnListChanged?.Invoke(_value);
        }
    }

    public void Add(T item)
    {
        _value.Add(item);
        OnItemAdded?.Invoke(item);  
        OnListChanged?.Invoke(_value); 
    }

    // 리스트의 요소 제거
    public void Remove(T item)
    {
        if (_value.Remove(item))
        {
            OnItemRemoved?.Invoke(item);  
            OnListChanged?.Invoke(_value); 
        }
    }
    
    public void Clear()
    {
        _value.Clear();
        OnListCleared?.Invoke();
        OnListChanged?.Invoke(_value);  
    }
    
    public bool Contains(T item)
    {
        return _value.Contains(item);
    }

    public T this[int index]
    {
        get => _value[index];
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value[index], value))
            {
                _value[index] = value;
                OnListChanged?.Invoke(_value);
            }
        }
    }

    public int Count => _value.Count;
}

[Serializable]
public class SerializableBitSet
{
    private BitArray _bitArray;  // 내부적으로 BitArray 사용
    [SerializeField] private List<int> activeIndexes;

    public SerializableBitSet(int size)
    {
        _bitArray = new BitArray(size, false);
        activeIndexes = new List<int>();
    }

    // 값 설정
    public void Set(int index, bool value)
    {
        if (IsValidIndex(index))
            _bitArray.Set(index, value);
    }

    // 값 가져오기
    public bool Get(int index)
    {
        return IsValidIndex(index) && _bitArray.Get(index);
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < _bitArray.Length;
    }

    // ✅ JSON 저장 전에 활성화된 인덱스를 리스트로 변환
    public void PrepareForSerialization()
    {
        activeIndexes.Clear();
        for (int i = 0; i < _bitArray.Length; i++)
        {
            if (_bitArray.Get(i))
            {
                activeIndexes.Add(i);
            }
        }
    }

    // ✅ JSON 로드 후 다시 BitArray로 복원
    public void RestoreFromSerialization(int bitArraySize)
    {
        _bitArray = new BitArray(bitArraySize, false);
        foreach (int index in activeIndexes)
        {
            if (IsValidIndex(index))
            {
                _bitArray.Set(index, true);
            }
        }
    }
}

