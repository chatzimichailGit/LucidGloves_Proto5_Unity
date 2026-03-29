using UnityEngine;

public class GloveFingerAnimator : MonoBehaviour
{
    public SerialTest serialReader;
    public Transform[] fingerJoints = new Transform[15]; // 3 joints per finger × 5
    public Transform[] splayJoints = new Transform[5];   // base joints (optional)

    void Update()
    {
        string line = serialReader.GetLatestGloveLine();
        if (string.IsNullOrEmpty(line)) return;

        var data = GloveParser.Parse(line);
        AnimateFingers(data);
    }

    void AnimateFingers(GloveFingerData data)
    {
        for (int i = 0; i < 5; i++) // 5 fingers
        {
            float bend = data.flexion[i];
            float splay = data.splay[i];

            for (int j = 0; j < 3; j++) // 3 joints: proximal, intermediate, distal
            {
                int idx = i * 3 + j;
                float angle = bend * (j == 0 ? 45f : 30f); // base joint bends more
                if (fingerJoints[idx])
                    fingerJoints[idx].localRotation = Quaternion.Euler(angle, 0, 0);
            }

            if (i < splayJoints.Length && splayJoints[i])
            {
                float splayAngle = splay * 15f;
                splayJoints[i].localRotation = Quaternion.Euler(0, splayAngle, 0);
            }
        }
    }
}
