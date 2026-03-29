using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.Hands;

public class CustomHandSkeletonDriver : MonoBehaviour
{
    [Serializable]
    public struct JointToTransformReference
    {
        public XRHandJointID xrHandJointID;
        public Transform jointTransform;
    }

    [SerializeField]
    XRHandTrackingEvents m_XRHandTrackingEvents;

    [SerializeField]
    Transform m_RootTransform;

    [SerializeField]
    List<JointToTransformReference> m_JointTransformReferences;

    Transform[] m_JointTransforms;
    bool[] m_HasJointTransformMask;
    NativeArray<Pose> m_JointLocalPoses;
    bool m_HasRootTransform;

    void OnEnable()
    {
        m_JointLocalPoses = new NativeArray<Pose>(XRHandJointID.EndMarker.ToIndex(), Allocator.Persistent);

        if (m_XRHandTrackingEvents == null)
            TryGetComponent(out m_XRHandTrackingEvents);

        if (m_XRHandTrackingEvents == null)
        {
            Debug.LogError("CustomHandSkeletonDriver requires XRHandTrackingEvents", this);
            return;
        }

        SubscribeToHandTrackingEvents();
        InitializeFromSerializedReferences();
    }

    void OnDisable()
    {
        if (m_JointLocalPoses.IsCreated)
            m_JointLocalPoses.Dispose();

        UnsubscribeFromHandTrackingEvents();
    }

    void SubscribeToHandTrackingEvents()
    {
        m_XRHandTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);
        m_XRHandTrackingEvents.poseUpdated.AddListener(OnRootPoseUpdated);
    }

    void UnsubscribeFromHandTrackingEvents()
    {
        m_XRHandTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);
        m_XRHandTrackingEvents.poseUpdated.RemoveListener(OnRootPoseUpdated);
    }

    void OnRootPoseUpdated(Pose rootPose)
    {
        if (!m_HasRootTransform) return;

        m_RootTransform.localPosition = rootPose.position;
        m_RootTransform.localRotation = rootPose.rotation;
    }

    void OnJointsUpdated(XRHandJointsUpdatedEventArgs args)
    {
        UpdateJointLocalPoses(args);
        ApplyUpdatedTransformPoses();
    }

    void UpdateJointLocalPoses(XRHandJointsUpdatedEventArgs args)
    {
        var wristJoint = args.hand.GetJoint(XRHandJointID.Wrist);
        if (wristJoint.TryGetPose(out Pose wristPose))
        {
            int wristIndex = XRHandJointID.Wrist.ToIndex();
            m_JointLocalPoses[wristIndex] = wristPose;
        }

        var palmJoint = args.hand.GetJoint(XRHandJointID.Palm);
        if (palmJoint.TryGetPose(out Pose palmPose))
        {
            int palmIndex = XRHandJointID.Palm.ToIndex();
            m_JointLocalPoses[palmIndex] = palmPose;
        }
    }

    void ApplyUpdatedTransformPoses()
    {
        int wristIndex = XRHandJointID.Wrist.ToIndex();
        int palmIndex = XRHandJointID.Palm.ToIndex();

        if (m_HasJointTransformMask[wristIndex] && m_JointTransforms[wristIndex] != null)
        {
            m_JointTransforms[wristIndex].localPosition = m_JointLocalPoses[wristIndex].position;
            m_JointTransforms[wristIndex].localRotation = m_JointLocalPoses[wristIndex].rotation;
        }

        if (m_HasJointTransformMask[palmIndex] && m_JointTransforms[palmIndex] != null)
        {
            m_JointTransforms[palmIndex].localPosition = m_JointLocalPoses[palmIndex].position;
            m_JointTransforms[palmIndex].localRotation = m_JointLocalPoses[palmIndex].rotation;
        }
    }

    public void InitializeFromSerializedReferences()
    {
        if (m_RootTransform != null)
            m_HasRootTransform = true;

        m_JointTransforms = new Transform[XRHandJointID.EndMarker.ToIndex()];
        m_HasJointTransformMask = new bool[XRHandJointID.EndMarker.ToIndex()];

        foreach (var joint in m_JointTransformReferences)
        {
            var id = joint.xrHandJointID;
            if (id != XRHandJointID.Wrist && id != XRHandJointID.Palm)
                continue;

            int index = id.ToIndex();
            m_JointTransforms[index] = joint.jointTransform;
            m_HasJointTransformMask[index] = joint.jointTransform != null;
        }
    }
}
