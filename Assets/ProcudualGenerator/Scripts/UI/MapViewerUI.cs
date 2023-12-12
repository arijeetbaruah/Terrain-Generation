using TMPro;
using UnityEngine;

public class MapViewerUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField rotationSpeed;
    [SerializeField] private CameraRotator cameraRotator;

    private void Awake()
    {
        rotationSpeed.text = cameraRotator.cameraRotationDuration.ToString();
    }

    private void OnEnable()
    {
        rotationSpeed.onValueChanged.AddListener(val =>
        {
            cameraRotator.UpdateDuration(float.Parse(val));
        });
    }
}
