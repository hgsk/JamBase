using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

// ScriptableObject for skill visual data
[CreateAssetMenu(fileName = "New Skill Visual Data", menuName = "Skill System/Skill Visual Data")]
public class SkillVisualDataSO : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public Color cooldownColor = Color.grey;
}

// Manager for skill UI elements
public class SkillUIManager : MonoBehaviour
{
    public GameObject skillIconPrefab;
    public Transform skillIconsParent;
    public GameObject commandFeedbackPrefab;
    public Transform commandFeedbackParent;

    private Dictionary<string, SkillIconController> skillIcons = new Dictionary<string, SkillIconController>();
    private CommandFeedbackController commandFeedback;

    private void Start()
    {
        InitializeSkillIcons();
        InitializeCommandFeedback();
    }

    private void InitializeSkillIcons()
    {
        var skillManager = GetComponent<SkillManager>();
        if (skillManager != null)
        {
            foreach (var skill in skillManager.GetSkills())
            {
                CreateSkillIcon(skill);
            }
        }
    }

    private void CreateSkillIcon(ISkill skill)
    {
        var iconObject = Instantiate(skillIconPrefab, skillIconsParent);
        var iconController = iconObject.GetComponent<SkillIconController>();
        if (iconController != null)
        {
            iconController.Initialize(skill);
            skillIcons[skill.Name] = iconController;
        }
    }

    private void InitializeCommandFeedback()
    {
        var feedbackObject = Instantiate(commandFeedbackPrefab, commandFeedbackParent);
        commandFeedback = feedbackObject.GetComponent<CommandFeedbackController>();
    }

    public void UpdateSkillCooldown(string skillName, float cooldownPercentage)
    {
        if (skillIcons.TryGetValue(skillName, out var iconController))
        {
            iconController.UpdateCooldown(cooldownPercentage);
        }
    }

    public void ShowCommandFeedback(List<string> currentSequence, float timeRemaining)
    {
        if (commandFeedback != null)
        {
            commandFeedback.UpdateFeedback(currentSequence, timeRemaining);
        }
    }
}

// Controller for individual skill icons
public class SkillIconController : MonoBehaviour
{
    public Image iconImage;
    public Image cooldownOverlay;
    public TextMeshProUGUI skillNameText;

    private SkillVisualDataSO visualData;

    public void Initialize(ISkill skill)
    {
        visualData = Resources.Load<SkillVisualDataSO>($"SkillVisualData/{skill.Name}");
        if (visualData != null)
        {
            iconImage.sprite = visualData.icon;
            skillNameText.text = visualData.skillName;
            cooldownOverlay.color = visualData.cooldownColor;
        }
        else
        {
            Debug.LogWarning($"SkillVisualData not found for skill: {skill.Name}");
        }
    }

    public void UpdateCooldown(float cooldownPercentage)
    {
        cooldownOverlay.fillAmount = cooldownPercentage;
    }
}

// Controller for command input feedback
public class CommandFeedbackController : MonoBehaviour
{
    public TextMeshProUGUI sequenceText;
    public Image timerBar;

    public void UpdateFeedback(List<string> currentSequence, float timeRemaining)
    {
        sequenceText.text = string.Join(" ", currentSequence);
        timerBar.fillAmount = timeRemaining;
    }
}

// Extension of CommandInputManager to include UI updates
public class CommandInputManagerWithUI : CommandInputManager
{
    private SkillUIManager uiManager;

    void Start()
    {
        base.Start();
        uiManager = GetComponent<SkillUIManager>();
    }

    void Update()
    {
        base.Update();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uiManager != null)
        {
            foreach (var skill in GetComponent<SkillManager>().GetSkills())
            {
                uiManager.UpdateSkillCooldown(skill.Name, 1 - (skill.Cooldown / skill.MaxCooldown));
            }

            uiManager.ShowCommandFeedback(currentInputSequence, GetCurrentTimeLimit() - (Time.time - lastInputTime));
        }
    }
}

// Extension of SkillManager to provide access to skills
public class SkillManagerWithUI : SkillManager
{
    public IEnumerable<ISkill> GetSkills()
    {
        return skills;
    }
}
