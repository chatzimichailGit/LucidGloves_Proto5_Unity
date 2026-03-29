using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class FingerDistanceDebugger : MonoBehaviour
{
    private XRHandSubsystem handSubsystem;

    [SerializeField]
    private bool useLeftHand = true;

    void Start()
    {
        handSubsystem = GetHandSubsystem();
        if (handSubsystem == null)
            Debug.LogError("❌ XRHandSubsystem not found. Make sure XR Hands package is active and configured.");
    }

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running)
            return;

        XRHand hand = useLeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;

        if (!hand.isTracked)
            return;

        var thumb = hand.GetJoint(XRHandJointID.ThumbTip);
        var index = hand.GetJoint(XRHandJointID.IndexTip);

        if (thumb.TryGetPose(out Pose thumbPose) && index.TryGetPose(out Pose indexPose))
        {
            float distance = Vector3.Distance(thumbPose.position, indexPose.position);
            Debug.Log($"🧠 Distance: {distance:F3} meters");
        }
    }

    // Utility method to get XRHandSubsystem at runtime
    private XRHandSubsystem GetHandSubsystem()
    {
        XRHandSubsystem foundSubsystem = null;
        var subsystems = new System.Collections.Generic.List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        if (subsystems.Count > 0)
            foundSubsystem = subsystems[0];

        return foundSubsystem;
    }
}
