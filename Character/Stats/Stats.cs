using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewStatDefinition", menuName = "Character System/Stat Definition")]
public class StatDefinitionSO : ScriptableObject
{
    public string statName;
    public float minValue = 0f;
    public float maxValue = 100f;
}

[Serializable]
public class StatValue
{
    public StatDefinitionSO statDefinition;
    public float value;
}

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character System/Character")]
public class CharacterSO : ScriptableObject
{
    public string characterName;
    public string characterId; // 一意の識別子
    public List<StatValue> stats = new List<StatValue>();
}

[CreateAssetMenu(fileName = "NewRelationshipType", menuName = "Character System/Relationship Type")]
public class RelationshipTypeSO : ScriptableObject
{
    public string typeName;
    public float minValue = -100f;
    public float maxValue = 100f;
}

[Serializable]
public class Relationship
{
    public CharacterSO fromCharacter;
    public CharacterSO toCharacter;
    public RelationshipTypeSO relationshipType;
    public float value;
}

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Character System/Character Database")]
public class CharacterDatabaseSO : ScriptableObject
{
    public List<CharacterSO> characters = new List<CharacterSO>();
    public List<Relationship> relationships = new List<Relationship>();
}

public class CharacterManager : MonoBehaviour
{
    public CharacterDatabaseSO database;
    private Dictionary<string, CharacterSO> characterLookup = new Dictionary<string, CharacterSO>();
    private Dictionary<string, Dictionary<string, Dictionary<string, Relationship>>> relationshipLookup = new Dictionary<string, Dictionary<string, Dictionary<string, Relationship>>>();

    private void Awake()
    {
        InitializeCharacterLookup();
        InitializeRelationshipLookup();
    }

    private void InitializeCharacterLookup()
    {
        foreach (var character in database.characters)
        {
            characterLookup[character.characterId] = character;
        }
    }

    private void InitializeRelationshipLookup()
    {
        foreach (var relationship in database.relationships)
        {
            string fromId = relationship.fromCharacter.characterId;
            string toId = relationship.toCharacter.characterId;
            string typeId = relationship.relationshipType.typeName;

            if (!relationshipLookup.ContainsKey(fromId))
            {
                relationshipLookup[fromId] = new Dictionary<string, Dictionary<string, Relationship>>();
            }
            if (!relationshipLookup[fromId].ContainsKey(toId))
            {
                relationshipLookup[fromId][toId] = new Dictionary<string, Relationship>();
            }
            relationshipLookup[fromId][toId][typeId] = relationship;
        }
    }

    // キャラクターステータス関連のメソッド
    public float GetCharacterStat(string characterId, string statName)
    {
        if (characterLookup.TryGetValue(characterId, out CharacterSO character))
        {
            var stat = character.stats.Find(s => s.statDefinition.statName == statName);
            if (stat != null)
            {
                return stat.value;
            }
        }
        Debug.LogWarning($"Stat {statName} not found for character {characterId}");
        return 0f;
    }

    public void SetCharacterStat(string characterId, string statName, float value)
    {
        if (characterLookup.TryGetValue(characterId, out CharacterSO character))
        {
            var stat = character.stats.Find(s => s.statDefinition.statName == statName);
            if (stat != null)
            {
                stat.value = Mathf.Clamp(value, stat.statDefinition.minValue, stat.statDefinition.maxValue);
            }
            else
            {
                Debug.LogWarning($"Stat {statName} not found for character {characterId}");
            }
        }
        else
        {
            Debug.LogWarning($"Character {characterId} not found");
        }
    }

    public void ModifyCharacterStat(string characterId, string statName, float amount)
    {
        if (characterLookup.TryGetValue(characterId, out CharacterSO character))
        {
            var stat = character.stats.Find(s => s.statDefinition.statName == statName);
            if (stat != null)
            {
                SetCharacterStat(characterId, statName, stat.value + amount);
            }
        }
    }

    // 関係性関連のメソッド
    public float GetRelationshipValue(string fromCharacterId, string toCharacterId, string relationshipType)
    {
        if (TryGetRelationship(fromCharacterId, toCharacterId, relationshipType, out Relationship relationship))
        {
            return relationship.value;
        }
        return 0f;
    }

    public void SetRelationshipValue(string fromCharacterId, string toCharacterId, string relationshipType, float value)
    {
        if (TryGetRelationship(fromCharacterId, toCharacterId, relationshipType, out Relationship relationship))
        {
            relationship.value = Mathf.Clamp(value, relationship.relationshipType.minValue, relationship.relationshipType.maxValue);
        }
        else
        {
            Debug.LogWarning($"Relationship not found: {fromCharacterId} -> {toCharacterId} ({relationshipType})");
        }
    }

    public void ModifyRelationshipValue(string fromCharacterId, string toCharacterId, string relationshipType, float amount)
    {
        if (TryGetRelationship(fromCharacterId, toCharacterId, relationshipType, out Relationship relationship))
        {
            SetRelationshipValue(fromCharacterId, toCharacterId, relationshipType, relationship.value + amount);
        }
    }

    private bool TryGetRelationship(string fromCharacterId, string toCharacterId, string relationshipType, out Relationship relationship)
    {
        relationship = null;
        return relationshipLookup.TryGetValue(fromCharacterId, out var toCharacters) &&
               toCharacters.TryGetValue(toCharacterId, out var relationshipTypes) &&
               relationshipTypes.TryGetValue(relationshipType, out relationship);
    }
}

// UI更新用のスクリプト
public class CharacterSystemUI : MonoBehaviour
{
    public CharacterManager characterManager;
    
    // UIコンポーネントへの参照をここに追加

    public void UpdateStatUI(string characterId, string statName, float newValue)
    {
        characterManager.SetCharacterStat(characterId, statName, newValue);
        // UIの更新ロジックをここに実装
    }

    public void UpdateRelationshipUI(string fromCharacterId, string toCharacterId, string relationshipType, float newValue)
    {
        characterManager.SetRelationshipValue(fromCharacterId, toCharacterId, relationshipType, newValue);
        // UIの更新ロジックをここに実装
    }
}
