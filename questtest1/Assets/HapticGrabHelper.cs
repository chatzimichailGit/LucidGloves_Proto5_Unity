using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class HapticGrabHelper : MonoBehaviour
{
    public int maxHaptic = 1000;  // Fully open
    public int minHaptic = 200;   // Strongest resistance

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;
    private bool isGrabbed = false;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
        interactable.selectEntered.AddListener(OnGrabStart);
        interactable.selectExited.AddListener(OnGrabEnd);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        SendHapticsFromVolume();
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (!isGrabbed)
        {
            ReleaseHaptics();
        }
    }

    private void OnGrabStart(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        SendHapticsFromVolume(); // Optional
    }

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        isGrabbed = false;
        ReleaseHaptics();
    }

    private void SendHapticsFromVolume()
    {
        float volume = GetObjectVolume();
        float normalizedVolume = Mathf.Clamp01(Mathf.InverseLerp(0.001f, 0.1f, volume));  // 0 = small, 1 = large

        float strength = 1f - normalizedVolume; // smaller object = more strength
        int value = Mathf.RoundToInt(Mathf.Lerp(minHaptic, maxHaptic, strength));

        int[] hapticValues = new int[5] { value, value, value, value, value };
        SerialTest.Instance.SendHaptics(hapticValues);

        //Debug.Log($"[HAPTIC] Volume={volume:F4}, Normalized={normalizedVolume:F2}, Value Sent={value}");
    }

    private void ReleaseHaptics()
    {
        int[] relaxed = new int[5] { 1000, 1000, 1000, 1000, 1000 };
        SerialTest.Instance.SendHaptics(relaxed);
       // Debug.Log("[HAPTIC] Released (relaxed state)");
    }

    private float GetObjectVolume()
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
}
