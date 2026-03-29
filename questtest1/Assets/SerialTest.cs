using System.IO.Ports;
using UnityEngine;

public class SerialTest : MonoBehaviour
{
    public static SerialTest Instance;

    private SerialPort port;
    private string latestLine = "";

    public string portName = "COM9";  // Change to your actual port
    public int baudRate = 115200;

    void Awake()
    {
        // Singleton pattern so other scripts can call SendHaptics
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        try
        {
            port = new SerialPort(portName, baudRate);
            port.ReadTimeout = 50;
            port.Open();
            Debug.Log("✅ Serial Opened!");
            Invoke(nameof(ReadyToRead), 2f); // ESP32 boot wait
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Serial Error: " + e.Message);
        }
    }

    void ReadyToRead()
    {
        Debug.Log("🟢 Ready to read glove data from ESP32.");
    }

    void Update()
    {
        if (port == null || !port.IsOpen) return;

        try
        {
            if (port.BytesToRead > 0)
            {
                string line = port.ReadLine().Trim();

                if (line.StartsWith("A") && line.Contains("(AB)"))
                {
                    latestLine = line;
                    // Optional: remove for performance
                     //Debug.Log("📨 Glove Data: " + line);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("⚠️ Serial timeout: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (port != null && port.IsOpen)
        {
            port.Close();
            Debug.Log("🔌 Serial Closed.");
        }
    }

    public string GetLatestGloveLine()
    {
        return latestLine;
    }

    // ✅ This sends haptic limits to the ESP32
    public void SendHaptics(int[] hapticLimits)
    {
        if (port == null || !port.IsOpen || hapticLimits.Length < 5) return;

        string message = $"A{hapticLimits[0]}B{hapticLimits[1]}C{hapticLimits[2]}D{hapticLimits[3]}E{hapticLimits[4]}\n";
        port.WriteLine(message);
        // Optional:
         Debug.Log("🚀 Sent to ESP32: " + message);
    }
}
