using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GloveFingerData
{
    public float[] flexion = new float[5]; // Thumb–Pinky
    public float[] splay = new float[5];   // AB–EB
}

public static class GloveParser
{
    static Regex regex = new(@"(\(?[A-Z]{1,2}\)?)(\d+)");
    static readonly string[] flexKeys = { "A", "B", "C", "D", "E" };
    static readonly string[] splayKeys = { "AB", "BB", "CB", "DB", "EB" };

    public static GloveFingerData Parse(string line)
    {
        var data = new GloveFingerData();
        var matches = regex.Matches(line);
        var map = new Dictionary<string, int>();

        foreach (Match m in matches)
        {
            string key = m.Groups[1].Value.Trim('(', ')');
            int value = int.Parse(m.Groups[2].Value);
            if (!map.ContainsKey(key))
                map.Add(key, value);
        }

        for (int i = 0; i < 5; i++)
        {
            if (map.TryGetValue(flexKeys[i], out int flexVal))
                data.flexion[i] = Normalize(flexVal, 0, 4095); // Adjust min/max as needed
            else
                Debug.LogWarning($"Missing flexion key: {flexKeys[i]}");

            if (map.TryGetValue(splayKeys[i], out int splayVal))
                data.splay[i] = NormalizeSplay(splayVal);
            else
                Debug.LogWarning($"Missing splay key: {splayKeys[i]}");
        }

        return data;
    }

    static float Normalize(int val, int min, int max)
    {
        return Mathf.Clamp01((float)(val - min) / (max - min));
    }

    static float NormalizeSplay(int val)
    {
        return Mathf.Clamp((val - 3450f) / 200f, -1f, 1f); // Adjust if needed
    }
}
