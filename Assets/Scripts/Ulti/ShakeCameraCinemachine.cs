using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UniRx;
using System;
using Observer;

public class ShakeCameraCinemachine : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeAmplitude = 1.2f;
    [SerializeField] private float shakeFrequency = 2.0f;

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;
    private IDisposable shakeTimerDisposable;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise == null)
            {
                noise = virtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }
    }
    void OnEnable()
    {
        EventDispatcher.Instance?.RegisterListener(EventID.ShakeCamera, ShakeCamera);
    }
    void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener(EventID.ShakeCamera, ShakeCamera);
    }
    private void OnDestroy()
    {
        shakeTimerDisposable?.Dispose();
    }
    public void ShakeCamera(object data)
    {
        if(data==null)
        {
            if (noise != null)
            {
                noise.m_AmplitudeGain = shakeAmplitude;
                noise.m_FrequencyGain = shakeFrequency;

            // Dispose previous timer if exists
            shakeTimerDisposable?.Dispose();
            
            // Create new timer
                shakeTimerDisposable = Observable.Timer(System.TimeSpan.FromSeconds(shakeDuration))
                    .Subscribe(_ => StopShake());
            }
            return;
        }
        ShakeCameraData shakeCameraData = (ShakeCameraData)data;
        if (noise != null)
        {
            noise.m_AmplitudeGain = shakeCameraData.amplitude;
            noise.m_FrequencyGain = shakeCameraData.frequency;
            
            // Dispose previous timer if exists
            shakeTimerDisposable?.Dispose();
            
            // Create new timer
            shakeTimerDisposable = Observable.Timer(System.TimeSpan.FromSeconds(shakeCameraData.duration))
                .Subscribe(_ => StopShake());
        }
    }

    private void StopShake()
    {
        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }
    }
}
public struct ShakeCameraData
{
    public float duration;
    public float amplitude;
    public float frequency;
}
