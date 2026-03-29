using UnityEngine;
using UnityEngine.InputSystem; 

public class HapticTest : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            int[] hapticValues = new int[] { 1000, 1000, 1000, 1000, 1000 };
            SerialTest.Instance.SendHaptics(hapticValues);
            Debug.Log("🧪 Sent haptic test command.");
        }
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            int[] hapticValues = new int[] { 0, 0, 0, 0, 0 };
            SerialTest.Instance.SendHaptics(hapticValues);
            Debug.Log("🧪 Sent haptic test command.");
        }
    }
}
