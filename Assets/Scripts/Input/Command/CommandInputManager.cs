using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// ScriptableObject for defining a single skill command
[CreateAssetMenu(fileName = "New Skill Command", menuName = "Skill System/Skill Command")]
public class SkillCommandSO : ScriptableObject
{
    public string skillName;
    public List<string> commandSequence = new List<string>();
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
    List<InputAction> inputActions = new List<InputAction>();
    public SkillCommandSetSO skillCommandSet;

    public List<string> currentInputSequence = new List<string>();
    public float lastInputTime;

    private PlayerSkillUser skillUser;

    public void Start()
    {
        inputActions.Add(new InputAction("Skill", InputActionType.Button, "<Keyboard>/space"));
        inputActions.Add(new InputAction("Skill", InputActionType.Button, "<Keyboard>/Y"));
        inputActions.Add(new InputAction("Skill", InputActionType.Button, "<Keyboard>/A"));
        skillUser = GetComponent<PlayerSkillUser>();
        if (skillUser == null)
        {
            Debug.LogError("PlayerSkillUser component not found!");
        }
    }

    public void Update()
    {
        // Check for key inputs
        inputActions.ForEach(action => {
            if (action.triggered)
            {
                HandleKeyInput(action.name);
            }
        });

        // Check if the current sequence has timed out
        if (currentInputSequence.Count > 0 && Time.time - lastInputTime > GetCurrentTimeLimit())
        {
            ResetInputSequence();
        }
    }

    private void HandleKeyInput(string key)
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

    private bool IsSequenceMatch(List<string> input, List<string> command)
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

    internal float GetCurrentTimeLimit()
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
