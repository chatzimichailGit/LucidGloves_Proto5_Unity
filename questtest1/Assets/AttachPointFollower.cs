using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

public class AttachPointFollower : MonoBehaviour
{
    public XRHandSubsystem handSubsystem;
    public XRNode handNode = XRNode.LeftHand;
    public bool freezeAfterGrab = true;
    public bool isFrozen = false;

    private bool wasGrabbingLastFrame = false;

    void Update()
    {
        if (freezeAfterGrab && isFrozen) return;

        if (handSubsystem == null || !handSubsystem.running) return;

        XRHand hand = (handNode == XRNode.LeftHand) ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!hand.isTracked) return;

        var thumb = hand.GetJoint(XRHandJointID.ThumbTip);
        var index = hand.GetJoint(XRHandJointID.IndexTip);

        if (thumb.TryGetPose(out Pose thumbPose) && index.TryGetPose(out Pose indexPose))
        {
            Vector3 midPoint = (thumbPose.position + indexPose.position) / 2f;
            Quaternion rotation = Quaternion.LookRotation(indexPose.position - thumbPose.position);

            transform.SetPositionAndRotation(midPoint, rotation);
        }
    }

    public void Freeze() => isFrozen = true;
    public void Unfreeze() => isFrozen = false;
}
