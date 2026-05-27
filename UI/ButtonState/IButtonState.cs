using UnityEngine.EventSystems;

public interface IButtonState
{
    void Enter();
    void Exit();
    void OnPointerDown(PointerEventData eventData);
    void OnPointerUp(PointerEventData eventData);
    void OnBeginDrag(PointerEventData eventData);
    void OnDrag(PointerEventData eventData);
    void OnEndDrag(PointerEventData eventData);
    void Update();
}