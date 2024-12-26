using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
// CameraDefinitions.cs
// UnityEngineに依存しない定義
public record CameraTransform
{
    public Vector3Data Position { get; }
    public QuaternionData Rotation { get; }
}

public record Vector3Data
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
}

public record QuaternionData
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public float W { get; }
}

// CameraConfiguration.cs

[CreateAssetMenu(fileName = "NewCameraConfig", menuName = "Cameras/Camera Configuration")]
public class CameraConfiguration : ScriptableObject
{
    // カメラの基本設定
    [SerializeField] private string cameraId;
    [SerializeField] private CameraPriority priority = CameraPriority.Normal;
    [SerializeField] private float fieldOfView = 60f;
    [SerializeField] private bool isOrthographic;
    [SerializeField] private float orthographicSize = 5f;
    
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public string CameraId => cameraId;
    public CameraPriority Priority => priority;
    public float FieldOfView => fieldOfView;
    public bool IsOrthographic => isOrthographic;
    public float OrthographicSize => orthographicSize;
    public float TransitionDuration => transitionDuration;
    public AnimationCurve TransitionCurve => transitionCurve;

    public enum CameraPriority
    {
        VeryLow = 0,
        Low = 25,
        Normal = 50,
        High = 75,
        VeryHigh = 100
    }
}

[RequireComponent(typeof(Camera))]
public class CameraInstance : MonoBehaviour
{
    [SerializeField] private CameraConfiguration configuration;
    private Camera cameraComponent;

    public string CameraId => configuration.CameraId;
    public CameraConfiguration Configuration => configuration;
    public Camera CameraComponent => cameraComponent;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        ApplyConfiguration();
    }

    private void ApplyConfiguration()
    {
        cameraComponent.fieldOfView = configuration.FieldOfView;
        cameraComponent.orthographic = configuration.IsOrthographic;
        cameraComponent.orthographicSize = configuration.OrthographicSize;
        cameraComponent.enabled = false; // 初期状態では無効化
    }
}

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CameraInstance defaultCamera;
    
    private Dictionary<string, CameraInstance> cameras = new Dictionary<string, CameraInstance>();
    private CameraInstance activeCamera;
    private bool isTransitioning;

    private void Awake()
    {
        // シーン内の全てのCameraInstanceを検索して登録
        foreach (var cam in FindObjectsOfType<CameraInstance>())
        {
            RegisterCamera(cam);
        }

        // デフォルトカメラを有効化
        if (defaultCamera != null)
        {
            ActivateCamera(defaultCamera.CameraId);
        }
    }

    public void RegisterCamera(CameraInstance camera)
    {
        if (!cameras.ContainsKey(camera.CameraId))
        {
            cameras[camera.CameraId] = camera;
        }
    }

    public async Task SwitchCamera(string targetCameraId)
    {
        if (isTransitioning || !cameras.ContainsKey(targetCameraId))
            return;

        var targetCamera = cameras[targetCameraId];
        if (targetCamera == activeCamera)
            return;

        isTransitioning = true;
        
        // トランジション実行
        await TransitionBetweenCameras(activeCamera, targetCamera);
        
        // アクティブカメラを更新
        activeCamera = targetCamera;
        isTransitioning = false;
    }

    private async Task TransitionBetweenCameras(CameraInstance fromCamera, CameraInstance toCamera)
    {
        var transitionDuration = toCamera.Configuration.TransitionDuration;
        var transitionCurve = toCamera.Configuration.TransitionCurve;
        
        // トランジション開始時の設定
        toCamera.CameraComponent.enabled = true;
        
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = transitionCurve.Evaluate(elapsed / transitionDuration);
            
            // カメラのブレンド処理
            LerpCameraProperties(fromCamera.CameraComponent, toCamera.CameraComponent, t);
            
            elapsed += Time.deltaTime;
            await Task.Yield();
        }

        // トランジション完了時の処理
        fromCamera.CameraComponent.enabled = false;
        toCamera.CameraComponent.enabled = true;
    }

    private void LerpCameraProperties(Camera fromCam, Camera toCam, float t)
    {
        // カメラのプロパティを補間
        if (!fromCam.orthographic && !toCam.orthographic)
        {
            fromCam.fieldOfView = Mathf.Lerp(fromCam.fieldOfView, toCam.fieldOfView, t);
        }
        else if (fromCam.orthographic && toCam.orthographic)
        {
            fromCam.orthographicSize = Mathf.Lerp(fromCam.orthographicSize, toCam.orthographicSize, t);
        }

        // 位置と回転の補間
        fromCam.transform.position = Vector3.Lerp(fromCam.transform.position, toCam.transform.position, t);
        fromCam.transform.rotation = Quaternion.Lerp(fromCam.transform.rotation, toCam.transform.rotation, t);
    }

    public void ActivateCamera(string cameraId)
    {
        if (cameras.TryGetValue(cameraId, out var camera))
        {
            if (activeCamera != null)
                activeCamera.CameraComponent.enabled = false;
            
            camera.CameraComponent.enabled = true;
            activeCamera = camera;
        }
    }

    // 優先度に基づいてカメラを切り替える
    public void SwitchToHighestPriorityCamera()
    {
        var highestPriorityCamera = cameras.Values
            .OrderByDescending(c => c.Configuration.Priority)
            .FirstOrDefault();

        if (highestPriorityCamera != null)
        {
            SwitchCamera(highestPriorityCamera.CameraId);
        }
    }
}

// Example Usage:
// CameraDirector.cs
public class CameraDirector : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;

    public async void SwitchToGameplayCamera()
    {
        await cameraManager.SwitchCamera("GameplayCamera");
    }

    public async void SwitchToCutsceneCamera()
    {
        await cameraManager.SwitchCamera("CutsceneCamera");
    }
}