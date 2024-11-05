using UnityEngine;
using System;
using System.Collections.Generic;

// 既存のCharacterManagerインターフェース
public interface ICharacterManager
{
    float GetCharacterStat(string characterId, string statName);
    void SetCharacterStat(string characterId, string statName, float value);
    void ModifyCharacterStat(string characterId, string statName, float amount);
    float GetRelationshipValue(string fromCharacterId, string toCharacterId, string relationshipType);
    void SetRelationshipValue(string fromCharacterId, string toCharacterId, string relationshipType, float value);
    void ModifyRelationshipValue(string fromCharacterId, string toCharacterId, string relationshipType, float amount);
}

// 動的状態を表すクラス
public class DynamicState
{
    public string stateName;
    public float value;
    public float duration;
    public bool isPermanent;

    public DynamicState(string stateName, float value, float duration, bool isPermanent = false)
    {
        this.stateName = stateName;
        this.value = value;
        this.duration = duration;
        this.isPermanent = isPermanent;
    }
}

// CharacterManagerのDecorator
public class DynamicStateDecorator : ICharacterManager
{
    private ICharacterManager baseManager;
    private Dictionary<string, List<DynamicState>> dynamicStates = new Dictionary<string, List<DynamicState>>();

    public DynamicStateDecorator(ICharacterManager baseManager)
    {
        this.baseManager = baseManager;
    }

    public float GetCharacterStat(string characterId, string statName)
    {
        float baseStat = baseManager.GetCharacterStat(characterId, statName);
        return baseStat + GetDynamicStateModifier(characterId, statName);
    }

    public void SetCharacterStat(string characterId, string statName, float value)
    {
        baseManager.SetCharacterStat(characterId, statName, value);
    }

    public void ModifyCharacterStat(string characterId, string statName, float amount)
    {
        baseManager.ModifyCharacterStat(characterId, statName, amount);
    }

    public float GetRelationshipValue(string fromCharacterId, string toCharacterId, string relationshipType)
    {
        return baseManager.GetRelationshipValue(fromCharacterId, toCharacterId, relationshipType);
    }

    public void SetRelationshipValue(string fromCharacterId, string toCharacterId, string relationshipType, float value)
    {
        baseManager.SetRelationshipValue(fromCharacterId, toCharacterId, relationshipType, value);
    }

    public void ModifyRelationshipValue(string fromCharacterId, string toCharacterId, string relationshipType, float amount)
    {
        baseManager.ModifyRelationshipValue(fromCharacterId, toCharacterId, relationshipType, amount);
    }

    // 新しい動的状態管理メソッド
    public void AddDynamicState(string characterId, string stateName, float value, float duration, bool isPermanent = false)
    {
        if (!dynamicStates.ContainsKey(characterId))
        {
            dynamicStates[characterId] = new List<DynamicState>();
        }
        dynamicStates[characterId].Add(new DynamicState(stateName, value, duration, isPermanent));
    }

    public void RemoveDynamicState(string characterId, string stateName)
    {
        if (dynamicStates.ContainsKey(characterId))
        {
            dynamicStates[characterId].RemoveAll(s => s.stateName == stateName);
        }
    }

    public bool HasDynamicState(string characterId, string stateName)
    {
        return dynamicStates.ContainsKey(characterId) && dynamicStates[characterId].Exists(s => s.stateName == stateName);
    }

    private float GetDynamicStateModifier(string characterId, string statName)
    {
        float modifier = 0f;
        if (dynamicStates.ContainsKey(characterId))
        {
            foreach (var state in dynamicStates[characterId])
            {
                if (state.stateName == statName)
                {
                    modifier += state.value;
                }
            }
        }
        return modifier;
    }

    // 動的状態の更新（MonoBehaviourのUpdateメソッド内で呼び出す）
    public void UpdateDynamicStates()
    {
        foreach (var characterStates in dynamicStates)
        {
            for (int i = characterStates.Value.Count - 1; i >= 0; i--)
            {
                var state = characterStates.Value[i];
                if (!state.isPermanent)
                {
                    state.duration -= Time.deltaTime;
                    if (state.duration <= 0)
                    {
                        characterStates.Value.RemoveAt(i);
                    }
                }
            }
        }
    }
}

// DynamicStateDecoratorを使用するためのMonoBehaviour
public class DynamicStateManager : MonoBehaviour
{
    public CharacterManager baseManager; // 既存のCharacterManager
    private DynamicStateDecorator decorator;

    void Start()
    {
        decorator = new DynamicStateDecorator(baseManager);
    }

    void Update()
    {
        decorator.UpdateDynamicStates();
    }

    // DynamicStateDecoratorの機能を公開するメソッド
    public void AddDynamicState(string characterId, string stateName, float value, float duration, bool isPermanent = false)
    {
        decorator.AddDynamicState(characterId, stateName, value, duration, isPermanent);
    }

    public void RemoveDynamicState(string characterId, string stateName)
    {
        decorator.RemoveDynamicState(characterId, stateName);
    }

    public bool HasDynamicState(string characterId, string stateName)
    {
        return decorator.HasDynamicState(characterId, stateName);
    }

    public float GetEffectiveCharacterStat(string characterId, string statName)
    {
        return decorator.GetCharacterStat(characterId, statName);
    }
}
