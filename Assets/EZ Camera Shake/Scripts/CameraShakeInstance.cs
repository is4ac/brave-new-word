using UnityEngine;

namespace EZCameraShake
{
    public enum CameraShakeState { FadingIn, FadingOut, Sustained, Inactive }

    public class CameraShakeInstance
    {
        /// <summary>
        /// The intensity of the shake. It is recommended that you use ScaleMagnitude to alter the magnitude of a shake.
        /// </summary>
        public float magnitude;

        /// <summary>
        /// Roughness of the shake. It is recommended that you use ScaleRoughness to alter the roughness of a shake.
        /// </summary>
        public float roughness;

        /// <summary>
        /// How much influence this shake has over the local position axes of the camera.
        /// </summary>
        public Vector3 positionInfluence;

        /// <summary>
        /// How much influence this shake has over the local rotation axes of the camera.
        /// </summary>
        public Vector3 rotationInfluence;

        /// <summary>
        /// Should this shake be removed from the CameraShakeInstance list when not active?
        /// </summary>
        public bool deleteOnInactive = true;


        float _roughMod = 1, _magnMod = 1;
        float _fadeOutDuration, _fadeInDuration;
        bool _sustain;
        float _currentFadeTime;
        float _tick = 0;
        Vector3 _amt;

        /// <summary>
        /// Will create a new instance that will shake once and fade over the given number of seconds.
        /// </summary>
        /// <param name="magnitude">The intensity of the shake.</param>
        /// <param name="fadeOutTime">How long, in seconds, to fade out the shake.</param>
        /// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>
        public CameraShakeInstance(float magnitude, float roughness, float fadeInTime, float fadeOutTime)
        {
            this.magnitude = magnitude;
            _fadeOutDuration = fadeOutTime;
            _fadeInDuration = fadeInTime;
            this.roughness = roughness;
            if (fadeInTime > 0)
            {
                _sustain = true;
                _currentFadeTime = 0;
            }
            else
            {
                _sustain = false;
                _currentFadeTime = 1;
            }

            _tick = Random.Range(-100, 100);
        }

        /// <summary>
        /// Will create a new instance that will start a sustained shake.
        /// </summary>
        /// <param name="magnitude">The intensity of the shake.</param>
        /// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>
        public CameraShakeInstance(float magnitude, float roughness)
        {
            this.magnitude = magnitude;
            this.roughness = roughness;
            _sustain = true;

            _tick = Random.Range(-100, 100);
        }

        public Vector3 UpdateShake()
        {
            _amt.x = Mathf.PerlinNoise(_tick, 0) - 0.5f;
            _amt.y = Mathf.PerlinNoise(0, _tick) - 0.5f;
            _amt.z = Mathf.PerlinNoise(_tick, _tick) - 0.5f;

            if (_fadeInDuration > 0 && _sustain)
            {
                if (_currentFadeTime < 1)
                    _currentFadeTime += Time.deltaTime / _fadeInDuration;
                else if (_fadeOutDuration > 0)
                    _sustain = false;
            }

            if (!_sustain)
                _currentFadeTime -= Time.deltaTime / _fadeOutDuration;

            if (_sustain)
                _tick += Time.deltaTime * roughness * _roughMod;
            else
                _tick += Time.deltaTime * roughness * _roughMod * _currentFadeTime;

            return _amt * magnitude * _magnMod * _currentFadeTime;
        }

        /// <summary>
        /// Starts a fade out over the given number of seconds.
        /// </summary>
        /// <param name="fadeOutTime">The duration, in seconds, of the fade out.</param>
        public void StartFadeOut(float fadeOutTime)
        {
            if (fadeOutTime == 0)
                _currentFadeTime = 0;

            _fadeOutDuration = fadeOutTime;
            _fadeInDuration = 0;
            _sustain = false;
        }

        /// <summary>
        /// Starts a fade in over the given number of seconds.
        /// </summary>
        /// <param name="fadeInTime">The duration, in seconds, of the fade in.</param>
        public void StartFadeIn(float fadeInTime)
        {
            if (fadeInTime == 0)
                _currentFadeTime = 1;

            _fadeInDuration = fadeInTime;
            _fadeOutDuration = 0;
            _sustain = true;
        }

        /// <summary>
        /// Scales this shake's roughness while preserving the initial Roughness.
        /// </summary>
        public float ScaleRoughness
        {
            get { return _roughMod; }
            set { _roughMod = value; }
        }

        /// <summary>
        /// Scales this shake's magnitude while preserving the initial Magnitude.
        /// </summary>
        public float ScaleMagnitude
        {
            get { return _magnMod; }
            set { _magnMod = value; }
        }

        /// <summary>
        /// A normalized value (about 0 to about 1) that represents the current level of intensity.
        /// </summary>
        public float NormalizedFadeTime
        { get { return _currentFadeTime; } }

        bool IsShaking
        { get { return _currentFadeTime > 0 || _sustain; } }

        bool IsFadingOut
        { get { return !_sustain && _currentFadeTime > 0; } }

        bool IsFadingIn
        { get { return _currentFadeTime < 1 && _sustain && _fadeInDuration > 0; } }

        /// <summary>
        /// Gets the current state of the shake.
        /// </summary>
        public CameraShakeState CurrentState
        {
            get
            {
                if (IsFadingIn)
                    return CameraShakeState.FadingIn;
                else if (IsFadingOut)
                    return CameraShakeState.FadingOut;
                else if (IsShaking)
                    return CameraShakeState.Sustained;
                else
                    return CameraShakeState.Inactive;
            }
        }
    }
}