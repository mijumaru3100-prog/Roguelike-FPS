using UnityEngine;
using DG.Tweening;
public class ElevatorDoor : MonoBehaviour
{
    public Vector3 openPositionOffset; 
    public Vector3 openScale = new Vector3(0.1f, 1f, 1f); 
    public float duration = 0.8f; 
    private Vector3 _initialPosition;
    private Vector3 _initialScale;
    private bool _isOpen = false;
    void Awake()
    {
        _initialPosition = transform.localPosition;
        _initialScale = transform.localScale;
    }
    public void Open()
    {
        if(_isOpen) return;
        _isOpen = true;
        transform.DOKill(); 
        transform.DOLocalMove(_initialPosition + openPositionOffset, duration).SetEase(Ease.OutCubic);
        transform.DOScale(openScale, duration).SetEase(Ease.OutCubic);
    }
    public void Close()
    {
        if(!_isOpen) return;
        _isOpen = false;
        transform.DOKill();
        transform.DOLocalMove(_initialPosition, duration).SetEase(Ease.InCubic);
        transform.DOScale(_initialScale, duration).SetEase(Ease.InCubic);
    }
}