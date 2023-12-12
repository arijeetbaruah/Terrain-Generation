using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraRotator : MonoBehaviour, IDragHandler
{
    [SerializeField] private Transform cam;

    public float cameraRotationDuration = 5f;

    private Tween tween;

    private void Start()
    {
        StartAutoRotation();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.delta != Vector2.zero)
        {
            tween.Kill();
            cam.Rotate(0, eventData.delta.x * 3 * Time.deltaTime, 0);
            tween = DOVirtual.DelayedCall(3, StartAutoRotation);
            return;
        }
    }

    public void UpdateDuration(float cameraRotationDuration)
    {
        tween.Kill();
        this.cameraRotationDuration = cameraRotationDuration;
        StartAutoRotation();
    }

    public void StartAutoRotation()
    {
        tween = cam.DORotate(new Vector3(0, 360 - 1, 0), cameraRotationDuration, RotateMode.Fast);
        tween.SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear).SetRelative(true);
    }
}
