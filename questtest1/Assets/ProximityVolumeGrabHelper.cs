using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ProximityVolumeGrabHelper : MonoBehaviour
{
    [Header("XR Settings")]
    public XRBaseInteractable interactable;
    public float grabCheckInterval = 0.05f;

    [Header("Volume Grab Settings")]
    public float minVolume = 0.001f;
    public float maxVolume = 0.05f;
    public float minThreshold = 0.2f;
    public float maxThreshold = 0.8f;

    private float objectVolume;
    private float grabThreshold;
    private XRHandSubsystem handSubsystem;
    private bool isGrabbing = false;
    private IXRSelectInteractor grabbingInteractor;

    public static bool IsLeftHandGrabbing = false;

    void Start()
    {
        if (interactable == null)
            interactable = GetComponent<XRBaseInteractable>();

        handSubsystem = FindHandSubsystem();
        objectVolume = CalculateVolume();

        float normalizedVolume = Mathf.Clamp01(Mathf.InverseLerp(minVolume, maxVolume, objectVolume));
        grabThreshold = Mathf.Lerp(minThreshold, maxThreshold, normalizedVolume);

        InvokeRepeating(nameof(CheckGrabCondition), 0.1f, grabCheckInterval);
    }

    void CheckGrabCondition()
    {
        if (handSubsystem == null || !handSubsystem.running || !interactable.isHovered)
            return;

        XRHand hand = handSubsystem.leftHand;
        if (!hand.isTracked)
        {
            if (isGrabbing) ForceRelease();
            return;
        }

        var thumb = hand.GetJoint(XRHandJointID.ThumbTip);
        var index = hand.GetJoint(XRHandJointID.IndexTip);

        if (thumb.TryGetPose(out Pose thumbPose) && index.TryGetPose(out Pose indexPose))
        {
            float distance = Vector3.Distance(thumbPose.position, indexPose.position);
            float normalized = Mathf.Clamp01(Mathf.InverseLerp(0.18f, 0.04f, distance));

            if (normalized > grabThreshold && !isGrabbing && !IsLeftHandGrabbing)
            {
                var interactor = FindClosestInteractor();
                if (interactor != null && interactable.interactionManager != null)
                {
                    // ⬇️ Align attach transform to pinch position once
                    var attachPoint = interactor.attachTransform;
                    if (attachPoint != null)
                    {
                        Vector3 pinchPos = (thumbPose.position + indexPose.position) / 2f;
                        Quaternion pinchRot = Quaternion.LookRotation(indexPose.position - thumbPose.position);
                        attachPoint.SetPositionAndRotation(pinchPos, pinchRot);
                    }

                    var ixrInteractor = interactor as IXRSelectInteractor;
                    var ixrInteractable = interactable as IXRSelectInteractable;

                    interactable.interactionManager.SelectEnter(ixrInteractor, ixrInteractable);
                    isGrabbing = true;
                    grabbingInteractor = ixrInteractor;
                    IsLeftHandGrabbing = true;
                }
            }
            else if (normalized < (grabThreshold - 0.1f) && isGrabbing)
            {
                if (grabbingInteractor != null)
                {
                    var ixrInteractable = interactable as IXRSelectInteractable;
                    interactable.interactionManager.SelectExit(grabbingInteractor, ixrInteractable);
                    isGrabbing = false;
                    grabbingInteractor = null;
                    IsLeftHandGrabbing = false;
                }
            }
        }
    }

    void ForceRelease()
    {
        if (grabbingInteractor != null)
        {
            interactable.interactionManager.SelectExit(grabbingInteractor, interactable);
            grabbingInteractor = null;
        }
        isGrabbing = false;
        IsLeftHandGrabbing = false;
    }

    XRBaseInteractor FindClosestInteractor()
    {
        var interactors = Object.FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);
        foreach (var interactor in interactors)
        {
            if (interactor is XRDirectInteractor)
                return interactor;
        }
        return null;
    }

    float CalculateVolume()
    {
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
            return box.size.x * box.size.y * box.size.z;
        if (col is SphereCollider sphere)
            return (4f / 3f) * Mathf.PI * Mathf.Pow(sphere.radius, 3);
        if (col is CapsuleCollider cap)
            return Mathf.PI * Mathf.Pow(cap.radius, 2) * ((4f / 3f) * cap.radius + cap.height);
        return transform.localScale.x * transform.localScale.y * transform.localScale.z;
    }

    XRHandSubsystem FindHandSubsystem()
    {
        var list = new System.Collections.Generic.List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(list);
        return list.Count > 0 ? list[0] : null;
    }
}
