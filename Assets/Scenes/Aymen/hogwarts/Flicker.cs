// TorchFlicker.cs
using UnityEngine;
[RequireComponent(typeof(Light))]
public class TorchFlicker : MonoBehaviour
{
    public float minIntensity = 8f;
    public float maxIntensity = 15f;
    public float flickerSpeed = 10f;
    Light lt;
    void Start() => lt = GetComponent<Light>();
    void Update()
    {
        lt.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise(Time.time * flickerSpeed, 0));
    }
}