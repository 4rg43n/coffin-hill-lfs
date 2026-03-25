using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public enum UpdateMode { Update, FixedUpdate, LateUpdate }

    [SerializeField] private Transform target;
    [SerializeField] private float dampTime = 0.15f;
    [SerializeField] private UpdateMode updateMode = UpdateMode.LateUpdate;

    private Vector3 _velocity = Vector3.zero;

    private void Update()
    {
        if (updateMode == UpdateMode.Update) MoveCamera();
    }

    private void FixedUpdate()
    {
        if (updateMode == UpdateMode.FixedUpdate) MoveCamera();
    }

    private void LateUpdate()
    {
        if (updateMode == UpdateMode.LateUpdate) MoveCamera();
    }

    private void MoveCamera()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, dampTime);
    }
}
