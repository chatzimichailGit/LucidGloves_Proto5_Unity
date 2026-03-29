using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class HapticGrabResponder : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    public int maxHaptic = 1000;  // Max force for smallest objects
    public int minHaptic = 200;   // Min force for large objects

    private int[] hapticValues = new int[5];

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        float volume = GetObjectVolume();

        // Map volume to haptic intensity (smaller objects => stronger feedback)
        float strength = Mathf.InverseLerp(0.001f, 0.1f, volume); // Normalize volume
        strength = 1f - Mathf.Clamp01(strength); // Invert: smaller = stronger
        int value = Mathf.RoundToInt(Mathf.Lerp(minHaptic, maxHaptic, strength));

        for (int i = 0; i < hapticValues.Length; i++)
            hapticValues[i] = value;

        SerialTest.Instance.SendHaptics(hapticValues);
        Debug.Log($"Sent haptic force {value} for volume {volume:F4}");
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        SerialTest.Instance.SendHaptics(new int[5] { 0, 0, 0, 0, 0 });
        Debug.Log("Released object, haptics reset.");
    }

    private float GetObjectVolume()
    {
        var collider = GetComponent<Collider>();
        if (collider is BoxCollider box)
            return box.size.x * box.size.y * box.size.z;
        else if (collider is SphereCollider sphere)
            return (4f / 3f) * Mathf.PI * Mathf.Pow(sphere.radius, 3);
        else if (collider is CapsuleCollider cap)
            return Mathf.PI * Mathf.Pow(cap.radius, 2) * ((4f / 3f) * cap.radius + cap.height);
        else
            return transform.localScale.x * transform.localScale.y * transform.localScale.z;
    }
}
