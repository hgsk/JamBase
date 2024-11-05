using UnityEngine;
// 動く的
public class MovingTarget : BaseGimmick
{
    public float moveSpeed = 5f;
    public float moveDistance = 10f;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool movingToEnd = true;

    private void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + transform.right * moveDistance;
    }

    private void Update()
    {
        if (isActive)
        {
            Vector3 targetPosition = movingToEnd ? endPosition : startPosition;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (transform.position == targetPosition)
            {
                movingToEnd = !movingToEnd;
            }
        }
    }
}

