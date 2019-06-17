using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamShake : MonoBehaviour {

    public CinemachineVirtualCamera VirtualCamera;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;
    private IEnumerator coroutine;

    // Use this for initialization
    void Start () {
        if (VirtualCamera != null)
            virtualCameraNoise = VirtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
    }
	
    public void Shake(float shakeDuration, float shakeAmplitude, float shakeFrequency)
    {
        coroutine = StartShake(shakeDuration, shakeAmplitude, shakeFrequency);
        StartCoroutine(coroutine);
    }

    private IEnumerator StartShake(float shakeDuration, float shakeAmplitude, float shakeFrequency)
    {
        virtualCameraNoise.m_AmplitudeGain = shakeAmplitude;
        virtualCameraNoise.m_FrequencyGain = shakeFrequency;
        yield return new WaitForSeconds(shakeDuration);
        virtualCameraNoise.m_AmplitudeGain = 0f;
    }

    public void StartCamShaking(float shakeAmplitude, float shakeFrequency)
    {
        virtualCameraNoise.m_AmplitudeGain = shakeAmplitude;
        virtualCameraNoise.m_FrequencyGain = shakeFrequency;
    }

    public void StopCamShaking()
    {
        virtualCameraNoise.m_AmplitudeGain = 0f;
    }
}
