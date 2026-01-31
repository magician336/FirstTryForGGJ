using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("平滑配置")]
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private bool useSmoothing = true;

    private Camera _mainCamera;
    private Vector3 _targetPosition;
    private Vector3 _currentVelocity;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _mainCamera = Camera.main;
        if (_mainCamera != null)
        {
            _targetPosition = _mainCamera.transform.position;
        }
    }

    private void LateUpdate()
    {
        if (_mainCamera == null) return;

        if (useSmoothing)
        {
            // 使用平滑阻尼移动摄像机
            _mainCamera.transform.position = Vector3.SmoothDamp(
                _mainCamera.transform.position,
                _targetPosition,
                ref _currentVelocity,
                smoothTime
            );
        }
        else
        {
            _mainCamera.transform.position = _targetPosition;
        }
    }

    /// <summary>
    /// 切换摄像机到目标位置
    /// </summary>
    /// <param name="newPosition">目标坐标</param>
    /// <param name="instant">是否瞬间跳过平滑过程</param>
    public void TransitionTo(Vector3 newPosition, bool instant = false)
    {
        // 保持 Z 轴（通常是 -10），防止摄像机飞入地层
        _targetPosition = new Vector3(newPosition.x, newPosition.y, _mainCamera.transform.position.z);

        if (instant)
        {
            _mainCamera.transform.position = _targetPosition;
            _currentVelocity = Vector3.zero;
        }
    }
}
