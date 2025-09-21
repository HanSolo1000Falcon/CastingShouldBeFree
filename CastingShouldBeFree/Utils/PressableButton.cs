using System;
using UnityEngine;

namespace CastingShouldBeFree.Utils;

public class PressableButton : MonoBehaviour
{
    public Action OnPress;

    private const float DebounceTime = 0.2f;
    private float touchTime;

    private void Awake() => gameObject.SetLayer(UnityLayer.GorillaInteractable);

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - touchTime < DebounceTime)
            return;

        if (other.GetComponentInParent<GorillaTriggerColliderHandIndicator>() is not null)
        {
            touchTime = Time.time;
            OnPress?.Invoke();
            GorillaTagger.Instance.StartVibration(other.GetComponentInParent<GorillaTriggerColliderHandIndicator>().isLeftHand, 0.2f, 0.2f);
        }
    }
}