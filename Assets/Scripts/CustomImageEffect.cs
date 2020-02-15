using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CustomImageEffect : MonoBehaviour
{
    public Material EffectMaterial;
    [Range(0.01f, 0.5f)]
    public float NoiseFadeSpeed = 0.1f;
    [Range(0.01f, 1.0f)]
    public float NoiseMagnitude = 0.5f;

    private float noiseMag = 0f;

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            noiseMag = NoiseMagnitude;
        }
        SetNoiseMagnitude(noiseMag);

        if (noiseMag > 0)
            noiseMag -= Time.deltaTime * NoiseFadeSpeed;
        else
            noiseMag = 0;
    }

    public void SetNoiseMagnitude(float _noiseMag)
    {
        EffectMaterial.SetFloat("_NoiseMagnitude", _noiseMag);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, EffectMaterial);
    }
}
