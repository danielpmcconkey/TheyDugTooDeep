using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchGlow : MonoBehaviour {

    public float startIntensity = 0;
    public float targetIntensity = 5;
    public float smoothFactor = 0.5f;
    public float minFlickerSpeed = 0.1f;
    public float maxFlickerSpeed = 1.0f;
    ////public float minFlickerIntensityModifier = 0.25f;
    public float maxFlickerIntensityModifier = 0.25f;
    //public float maxFlickerMovement = 0.1f;

    //public enum WaveForm { sin, tri, sqr, saw, inv, noise };
    //public WaveForm waveform = WaveForm.sin;
    //public float baseStart = 0.0f; // start 
    //public float amplitude = 1.0f; // amplitude of the wave
    //public float phase = 0.0f; // start point inside on wave cycle
    //public float frequency = 0.5f; // cycle frequency per second

    private Color originalColor;
    



    private Light _light;
    private bool _isLit;
    private bool _isFadeInTriggered;
    private float _nextFlicker = 0;
    private float _flickerCounter = 0;
    //private float _flickerTargetIntensity;
    //private float _oldFlickerTargetIntensity;
    //private bool _isFlickerIntensityChangePositive;

    //private Vector3 _originalPosition;

    private void Awake()
    {
        _light = GetComponent<Light>();
        _light.intensity = 0.0f;
        _isLit = false;
        _isFadeInTriggered = false;
    }
    private void Start()
    {
        if(_light == null) _light = GetComponent<Light>();
        _light.intensity = 0.0f;
        //_isLit = false;
        //_isFadeInTriggered = false;
    }

    public void FadeIn()
    {
        //if (_light != null)
        //    _light.intensity = 0.0f;
        _isLit = false;
        _isFadeInTriggered = true;
    }


    //private void OnDisable()
    //{
    //    if(_light != null) _light.intensity = 0.0f;
    //    _isLit = false;
    //    _isFadeInTriggered = false;
    //}
    void OnEnable()
    {
        if (_light != null) _light.intensity = 0.0f;
        _isLit = false;
        _isFadeInTriggered = false;
        //_originalPosition = transform.localPosition;
        //_flickerTargetIntensity = targetIntensity;
        //_oldFlickerTargetIntensity = _flickerTargetIntensity;
        //originalColor = _light.color;
    }

    // Update is called once per frame
    void Update () {
        if (_isFadeInTriggered && !_isLit)
        {
            if (_light.intensity < targetIntensity)
            {
                _light.intensity += Mathf.Lerp(startIntensity, targetIntensity, Time.deltaTime * smoothFactor);
                if (_light.intensity >= targetIntensity)
                {
                    _isFadeInTriggered = false;
                    _isLit = true;
                    flickerLight();
                }
            }
        }
        else
        {
            //_light.color = originalColor * (EvalWave());
            if (_flickerCounter >= _nextFlicker) flickerLight();
            else _flickerCounter += Time.deltaTime;
            //else
            //{
            //    if (_light.intensity < _flickerTargetIntensity && _isFlickerIntensityChangePositive)
            //        _light.intensity += Mathf.Lerp(_light.intensity, _flickerTargetIntensity, Time.deltaTime *.001f);
            //    else if(_light.intensity > _flickerTargetIntensity && !_isFlickerIntensityChangePositive)
            //        _light.intensity -= Mathf.Lerp(_light.intensity, _flickerTargetIntensity, Time.deltaTime *.001f);
            //    else flickerLight();
            //}
        }
    }

    void flickerLight()
    {
        //_oldFlickerTargetIntensity = _flickerTargetIntensity;
        //_flickerTargetIntensity = Random.Range(targetIntensity - maxFlickerIntensityModifier, targetIntensity + maxFlickerIntensityModifier);
        //if (_flickerTargetIntensity > _oldFlickerTargetIntensity) _isFlickerIntensityChangePositive = true;
        //else _isFlickerIntensityChangePositive = false;

        //float newX = Random.Range(_originalPosition.x - maxFlickerMovement, _originalPosition.x + maxFlickerMovement);
        //float newY = Random.Range(_originalPosition.y - maxFlickerMovement, _originalPosition.y + maxFlickerMovement);
        //float newZ = Random.Range(_originalPosition.z - maxFlickerMovement, _originalPosition.z + maxFlickerMovement);
        //transform.position = new Vector3(newX, newY, newZ);
        // reset the flicker counter
        _flickerCounter = 0;
        _nextFlicker = Random.Range(minFlickerSpeed, maxFlickerSpeed);
        _light.intensity = Random.Range(targetIntensity - maxFlickerIntensityModifier, targetIntensity + maxFlickerIntensityModifier); ;
    }


    //float EvalWave()
    //{
    //    float x = (Time.time + phase) * frequency;
    //    float y;
    //    x = x - Mathf.Floor(x); // normalized value (0..1)

    //    if (waveform == WaveForm.sin)
    //    {

    //        y = Mathf.Sin(x * 2 * Mathf.PI);
    //    }
    //    else if (waveform == WaveForm.tri)
    //    {

    //        if (x < 0.5f)
    //            y = 4.0f * x - 1.0f;
    //        else
    //            y = -4.0f * x + 3.0f;
    //    }
    //    else if (waveform == WaveForm.sqr)
    //    {

    //        if (x < 0.5f)
    //            y = 1.0f;
    //        else
    //            y = -1.0f;
    //    }
    //    else if (waveform == WaveForm.saw)
    //    {

    //        y = x;
    //    }
    //    else if (waveform == WaveForm.inv)
    //    {

    //        y = 1.0f - x;
    //    }
    //    else if (waveform == WaveForm.noise)
    //    {

    //        y = 1f - (Random.value * 2);
    //    }
    //    else
    //    {
    //        y = 1.0f;
    //    }
    //    return (y * amplitude) + baseStart;
    //}
}
