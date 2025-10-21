using CastingShouldBeFree.Core.Interface;
using CastingShouldBeFree.Utils;
using UnityEngine;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public abstract class ModeHandlerBase : MonoBehaviour
{
    public static bool RollLock        = true;
    public static bool SnappySmoothing = false;

    protected Vector3 targetPosition;
    protected Quaternion targetRotation;
    
    protected Vector3 lastPosition;
    protected Quaternion lastRotation;

    public abstract string HandlerName { get; }

    protected int GetSmoothingFactor() =>
            -(CameraHandler.Instance.SmoothingFactor - (CoreHandler.Instance.MaxSmoothing + 1));

    protected float ParseAngularVelocity(Vector3 angularVelocity) => angularVelocity.magnitude / 18f + 1f;
    
    protected void HandleGenericSmoothing(float t)
    {
        if (CameraHandler.Instance.SmoothingFactor > 0)
        {
            int realSmoothingFactor = GetSmoothingFactor();
            
            if (SnappySmoothing)
            {
                Vector3 velocity = (targetPosition - lastPosition) / t;
                Vector3 angularVelocity = targetRotation.GetAngularVelocity(lastRotation, t);
                
                targetPosition = Vector3.Lerp(CameraHandler.Instance.transform.position, targetPosition, t * realSmoothingFactor * velocity.magnitude);
                targetRotation = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation,
                        t * realSmoothingFactor * ParseAngularVelocity(angularVelocity));
            }
            else
            {
                targetPosition = Vector3.Lerp(CameraHandler.Instance.transform.position, targetPosition, t * realSmoothingFactor);
                targetRotation = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation, t * realSmoothingFactor);
            }
        }
        
        lastRotation = CameraHandler.Instance.transform.rotation;
        lastPosition = CameraHandler.Instance.transform.position;
    }

    protected void SetCameraPositionAndRotation()
    {
        CameraHandler.Instance.transform.position = targetPosition;
        CameraHandler.Instance.transform.rotation = targetRotation;
    }
}