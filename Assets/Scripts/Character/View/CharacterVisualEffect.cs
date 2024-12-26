// CharacterVisualState.cs
// キャラクターの視覚的な状態を表すADT (Algebraic Data Type)
using UnityEngine;

public record CharacterVisualState
{
    public bool IsSelected;
    public bool IsActive;
    public bool IsStealth;
    public float Opacity;
    
    public static CharacterVisualState Default => new()
    {
        IsSelected = false,
        IsActive = true,
        IsStealth = false,
        Opacity = 1.0f
    };
}

// VisualEffectSettings.cs
// 視覚効果の設定を保持するScriptableObject

[CreateAssetMenu(fileName = "VisualEffectSettings", menuName = "Character/Visual Effect Settings")]
public class VisualEffectSettings : ScriptableObject
{
    [Header("選択状態の設定")]
    [SerializeField] private Color selectedHighlightColor = Color.yellow;
    [SerializeField] private float selectedHighlightIntensity = 1.2f;

    [Header("非アクティブ状態の設定")]
    [SerializeField] private float inactiveOpacity = 0.5f;
    [SerializeField] private Color greyoutTint = Color.gray;

    [Header("ステルス状態の設定")]
    [SerializeField] private float stealthOpacity = 0.3f;
    [SerializeField] private Color stealthTint = new Color(0.5f, 0.5f, 1f, 0.5f);

    // 設定値へのアクセサ
    public Color SelectedHighlightColor => selectedHighlightColor;
    public float SelectedHighlightIntensity => selectedHighlightIntensity;
    public float InactiveOpacity => inactiveOpacity;
    public Color GreyoutTint => greyoutTint;
    public float StealthOpacity => stealthOpacity;
    public Color StealthTint => stealthTint;
}

// IVisualEffectProcessor.cs
// UnityEngineに依存しない視覚効果の処理インターフェース
public interface IVisualEffectProcessor
{
    CharacterVisualState ProcessVisualState(CharacterVisualState currentState);
}

// CharacterVisualController.cs
// UnityEngineに依存する実装部分

[RequireComponent(typeof(SpriteRenderer))]
public class CharacterVisualController : MonoBehaviour
{
    [SerializeField] private VisualEffectSettings effectSettings;
    
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private CharacterVisualState currentState;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        currentState = CharacterVisualState.Default;
    }

    public void UpdateVisualState(CharacterVisualState newState)
    {
        currentState = newState;
        ApplyVisualEffects();
    }

    private void ApplyVisualEffects()
    {
        // ハイライト効果の適用
        if (currentState.IsSelected)
        {
            propertyBlock.SetColor("_Color", effectSettings.SelectedHighlightColor);
            spriteRenderer.GetPropertyBlock(propertyBlock);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        // 非アクティブ状態の適用
        if (!currentState.IsActive)
        {
            var color = spriteRenderer.color;
            color *= effectSettings.GreyoutTint;
            color.a = effectSettings.InactiveOpacity;
            spriteRenderer.color = color;
        }

        // ステルス状態の適用
        if (currentState.IsStealth)
        {
            var color = spriteRenderer.color;
            color *= effectSettings.StealthTint;
            color.a = effectSettings.StealthOpacity;
            spriteRenderer.color = color;
        }
    }
}