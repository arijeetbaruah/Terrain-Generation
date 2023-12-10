using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    private Vector3 rot = Vector3.zero;

    [SerializeField] private float dayCycle = 20;

    private void FixedUpdate()
    {
        rot.x = dayCycle * Time.fixedDeltaTime;
        transform.Rotate(rot, Space.World);
    }
}
