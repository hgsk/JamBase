using UnityEngine;
using System.Collections.Generic;
using System;

// Command Input Manager
public class CommandInputManager : MonoBehaviour
{
    [Serializable]
    public class SkillCommand
    {
        public string skillName;
        public List<KeyCode> commandSequence;
        public float timeLimit = 2f; // Time limit to input the command
    }

    public List<SkillCommand> skillCommands = new List<SkillCommand>();

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
        if (currentInputSequence.Count > 0 && Time.time - lastInputTime > skillCommands[0].timeLimit)
        {
            ResetInputSequence();
        }
    }

    private void HandleKeyInput(KeyCode key)
    {
        currentInputSequence.Add(key);
        lastInputTime = Time.time;

        // Check if the current sequence matches any skill command
        foreach (var command in skillCommands)
        {
            if (IsSequenceMatch(currentInputSequence, command.commandSequence))
            {
                skillUser.UseSkill(command.skillName);
                ResetInputSequence();
                return;
            }
        }

        // If the sequence is longer than any command, reset it
        if (currentInputSequence.Count > skillCommands[0].commandSequence.Count)
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
}

// Updated PlayerSkillUser to work with CommandInputManager
[RequireComponent(typeof(SkillManager), typeof(CommandInputManager))]
public class PlayerSkillUser : MonoBehaviour, ISkillUser
{
    private SkillManager skillManager;

    private void Awake()
    {
        skillManager = GetComponent<SkillManager>();
    }

    public void UseSkill(string skillName)
    {
        skillManager.UseSkill(skillName);
    }
}

// Example of how to set up skill commands in the Unity Inspector
[System.Serializable]
public class SkillCommandSetup
{
    public string skillName;
    public List<KeyCode> commandSequence;
    public float timeLimit = 2f;
}

// MonoBehaviour to set up skill commands in the Unity Inspector
public class SkillCommandSetup : MonoBehaviour
{
    public List<SkillCommandSetup> skillCommands;

    private void Start()
    {
        var commandInputManager = GetComponent<CommandInputManager>();
        if (commandInputManager != null)
        {
            foreach (var setup in skillCommands)
            {
                commandInputManager.skillCommands.Add(new CommandInputManager.SkillCommand
                {
                    skillName = setup.skillName,
                    commandSequence = setup.commandSequence,
                    timeLimit = setup.timeLimit
                });
            }
        }
    }
}
