using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// 基本的な設定項目の抽象クラス
public abstract class SettingItem<T> : ScriptableObject
{
    public string key;
    public T defaultValue;
    public T currentValue;

    public abstract void Load();
    public abstract void Save();
    public abstract void Apply();
}

// float型の設定項目
[CreateAssetMenu(fileName = "FloatSetting", menuName = "Game/Settings/Float Setting")]
public class FloatSettingItem : SettingItem<float>
{
    public override void Load()
    {
        currentValue = PlayerPrefs.GetFloat(key, defaultValue);
    }

    public override void Save()
    {
        PlayerPrefs.SetFloat(key, currentValue);
    }

    public override void Apply()
    {
        // 具体的な適用ロジックはサブクラスで実装
    }
}

// int型の設定項目
[CreateAssetMenu(fileName = "IntSetting", menuName = "Game/Settings/Int Setting")]
public class IntSettingItem : SettingItem<int>
{
    public override void Load()
    {
        currentValue = PlayerPrefs.GetInt(key, defaultValue);
    }

    public override void Save()
    {
        PlayerPrefs.SetInt(key, currentValue);
    }

    public override void Apply()
    {
        // 具体的な適用ロジックはサブクラスで実装
    }
}

// bool型の設定項目
[CreateAssetMenu(fileName = "BoolSetting", menuName = "Game/Settings/Bool Setting")]
public class BoolSettingItem : SettingItem<bool>
{
    public override void Load()
    {
        currentValue = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }

    public override void Save()
    {
        PlayerPrefs.SetInt(key, currentValue ? 1 : 0);
    }

    public override void Apply()
    {
        // 具体的な適用ロジックはサブクラスで実装
    }
}

// 具体的な設定項目の例
[CreateAssetMenu(fileName = "MasterVolumeSetting", menuName = "Game/Settings/Master Volume")]
public class MasterVolumeSetting : FloatSettingItem
{
    public override void Apply()
    {
        AudioListener.volume = currentValue;
    }
}

[CreateAssetMenu(fileName = "GraphicsQualitySetting", menuName = "Game/Settings/Graphics Quality")]
public class GraphicsQualitySetting : IntSettingItem
{
    public override void Apply()
    {
        QualitySettings.SetQualityLevel(currentValue);
    }
}

[CreateAssetMenu(fileName = "FullScreenSetting", menuName = "Game/Settings/Full Screen")]
public class FullScreenSetting : BoolSettingItem
{
    public override void Apply()
    {
        Screen.fullScreen = currentValue;
    }
}

// 設定管理クラス
public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance;

    public SettingItem<float>[] floatSettings;
    public SettingItem<int>[] intSettings;
    public SettingItem<bool>[] boolSettings;

    public UnityEvent OnSettingsChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadAllSettings()
    {
        foreach (var setting in floatSettings) setting.Load();
        foreach (var setting in intSettings) setting.Load();
        foreach (var setting in boolSettings) setting.Load();
        ApplyAllSettings();
    }

    public void SaveAllSettings()
    {
        foreach (var setting in floatSettings) setting.Save();
        foreach (var setting in intSettings) setting.Save();
        foreach (var setting in boolSettings) setting.Save();
        PlayerPrefs.Save();
        ApplyAllSettings();
    }

    public void ApplyAllSettings()
    {
        foreach (var setting in floatSettings) setting.Apply();
        foreach (var setting in intSettings) setting.Apply();
        foreach (var setting in boolSettings) setting.Apply();
        OnSettingsChanged.Invoke();
    }
}

// UI管理の基底クラス
public abstract class SettingUIBase<T, U> : MonoBehaviour where U : SettingItem<T>
{
    public U settingItem;

    protected abstract void UpdateUI();
    protected abstract void OnValueChanged(T newValue);

    protected virtual void Start()
    {
        GameSettingsManager.Instance.OnSettingsChanged.AddListener(UpdateUI);
        UpdateUI();
    }

    protected void SaveSetting(T newValue)
    {
        settingItem.currentValue = newValue;
        GameSettingsManager.Instance.SaveAllSettings();
    }
}

// Sliderを使用する設定UI
public class SliderSettingUI : SettingUIBase<float, FloatSettingItem>
{
    public Slider slider;

    protected override void Start()
    {
        base.Start();
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    protected override void UpdateUI()
    {
        slider.value = settingItem.currentValue;
    }

    protected override void OnValueChanged(float newValue)
    {
        SaveSetting(newValue);
    }
}

// Dropdownを使用する設定UI
public class DropdownSettingUI : SettingUIBase<int, IntSettingItem>
{
    public Dropdown dropdown;

    protected override void Start()
    {
        base.Start();
        dropdown.onValueChanged.AddListener(OnValueChanged);
    }

    protected override void UpdateUI()
    {
        dropdown.value = settingItem.currentValue;
    }

    protected override void OnValueChanged(int newValue)
    {
        SaveSetting(newValue);
    }
}

// Toggleを使用する設定UI
public class ToggleSettingUI : SettingUIBase<bool, BoolSettingItem>
{
    public Toggle toggle;

    protected override void Start()
    {
        base.Start();
        toggle.onValueChanged.AddListener(OnValueChanged);
    }

    protected override void UpdateUI()
    {
        toggle.isOn = settingItem.currentValue;
    }

    protected override void OnValueChanged(bool newValue)
    {
        SaveSetting(newValue);
    }
}

// ゲーム状態管理クラス
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public GameState currentState;

    public UnityEvent<GameState> OnGameStateChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGameState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged.Invoke(currentState);
    }
}
