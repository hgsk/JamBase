using UnityEngine;
// 回転する障害物
public class RotatingObstacle : BaseGimmick
{
    public float rotationSpeed = 50f;
    public Vector3 rotationAxis = Vector3.up;

    private void Update()
    {
        if (isActive)
        {
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
        }
    }
}

