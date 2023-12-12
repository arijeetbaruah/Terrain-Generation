using TMPro;
using UnityEngine;

public class MapViewerUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField rotationSpeed;
    [SerializeField] private CameraRotator cameraRotator;

    private void Awake()
    {
        rotationSpeed.text = (1f / cameraRotator.cameraRotationDuration).ToString();
    }

    private void OnEnable()
    {
        rotationSpeed.onValueChanged.AddListener(val =>
        {
            if (string.IsNullOrEmpty(val))
            {
                cameraRotator.UpdateDuration(0);
                return;
            }
            cameraRotator.UpdateDuration(1 / float.Parse(val));
        });
    }
}
