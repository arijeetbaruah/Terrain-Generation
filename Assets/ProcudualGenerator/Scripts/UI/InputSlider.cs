using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputSlider : MonoBehaviour, IDragHandler
{
    [SerializeField] private TMP_InputField txt;
    [SerializeField] private float delta = 0.5f;

    public void OnDrag(PointerEventData eventData)
    {
        float val = 0;
        float.TryParse(txt.text, out val);

        if (eventData.delta.y > 0)
        {
            val -= delta;
        }
        else if (eventData.delta.y < 0)
        {
            val += delta;
        }

        txt.text = val.ToString();
    }
}
