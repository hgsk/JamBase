using UnityEngine;
using System;
using System.Collections.Generic;

// ScriptableObject for defining a single skill command
[CreateAssetMenu(fileName = "New Skill Command", menuName = "Skill System/Skill Command")]
public class SkillCommandSO : ScriptableObject
{
    public string skillName;
    public List<KeyCode> commandSequence = new List<KeyCode>();
    public float timeLimit = 2f;
}

// ScriptableObject for defining a set of skill commands
[CreateAssetMenu(fileName = "New Skill Command Set", menuName = "Skill System/Skill Command Set")]
public class SkillCommandSetSO : ScriptableObject
{
    public List<SkillCommandSO> skillCommands = new List<SkillCommandSO>();
}

// Updated CommandInputManager to use ScriptableObjects
public class CommandInputManager : MonoBehaviour
{
    public SkillCommandSetSO skillCommandSet;

    private List<KeyCode> currentInputSequence = new List<KeyCode>();
    private float lastInputTime;

    private PlayerSkillUser skillUser;

    private void Start()
    {
        skillUser = GetComponent<PlayerSkillUser>();
        if (skillUser == null)
        {
            Debug.LogError("PlayerSkillUser component not found!");
        }
    }

    private void Update()
    {
        // Check for key inputs
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                HandleKeyInput(key);
            }
        }

        // Check if the current sequence has timed out
        if (currentInputSequence.Count > 0 && Time.time - lastInputTime > GetCurrentTimeLimit())
        {
            ResetInputSequence();
        }
    }

    private void HandleKeyInput(KeyCode key)
    {
        currentInputSequence.Add(key);
        lastInputTime = Time.time;

        // Check if the current sequence matches any skill command
        foreach (var command in skillCommandSet.skillCommands)
        {
            if (IsSequenceMatch(currentInputSequence, command.commandSequence))
            {
                skillUser.UseSkill(command.skillName);
                ResetInputSequence();
                return;
            }
        }

        // If the sequence is longer than any command, reset it
        if (currentInputSequence.Count > GetLongestCommandLength())
        {
            ResetInputSequence();
        }
    }

    private bool IsSequenceMatch(List<KeyCode> input, List<KeyCode> command)
    {
        if (input.Count != command.Count) return false;
        for (int i = 0; i < input.Count; i++)
        {
            if (input[i] != command[i]) return false;
        }
        return true;
    }

    private void ResetInputSequence()
    {
        currentInputSequence.Clear();
    }

    private float GetCurrentTimeLimit()
    {
        return skillCommandSet.skillCommands.Count > 0 ? skillCommandSet.skillCommands[0].timeLimit : 2f;
    }

    private int GetLongestCommandLength()
    {
        int maxLength = 0;
        foreach (var command in skillCommandSet.skillCommands)
        {
            maxLength = Mathf.Max(maxLength, command.commandSequence.Count);
        }
        return maxLength;
    }
}

// PlayerSkillUser remains unchanged

// Example of how to use the ScriptableObjects in a MonoBehaviour
public class SkillCommandSetup : MonoBehaviour
{
    public SkillCommandSetSO skillCommandSet;

    private void Start()
    {
        var commandInputManager = GetComponent<CommandInputManager>();
        if (commandInputManager != null)
        {
            commandInputManager.skillCommandSet = skillCommandSet;
        }
    }
}
