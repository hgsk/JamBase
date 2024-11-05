using UnityEngine;
// 浮遊プラットフォーム
public class FloatingPlatform : BaseGimmick
{
    public float floatHeight = 5f;
    public float floatSpeed = 1f;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (isActive)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}

