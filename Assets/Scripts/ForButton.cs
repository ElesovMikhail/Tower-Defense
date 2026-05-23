using UnityEngine;
using UnityEngine.EventSystems;

public class ForButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private GameObject Text;

    private void Awake()
    {
        Text = transform.GetChild(0).gameObject;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Text.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Text.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Text.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Text.SetActive(false);
    }
}
