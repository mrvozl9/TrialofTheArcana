using UnityEngine;
using System.Collections;

public class PlayerAudioController : MonoBehaviour
{
    [Header("Looping Audio Sources")]
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioSource windSource;
    [SerializeField] private AudioSource floatSource;

    [Header("One-Shot SFX Audio Source (also used for voice)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip[] jumpClips;
    [SerializeField] private AudioClip[] landClips;
    [SerializeField] private AudioClip[] pogoClips;
    [SerializeField] private AudioClip[] dashClips;
    [SerializeField] private AudioClip[] attackClips; // ✅ NEW: Attack/Shooting sounds
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;

    [Header("Voice Lines")]
    [SerializeField] private AudioClip[] idleVoiceLines;
    [SerializeField] private AudioClip[] dashVoiceLines;
    [SerializeField] private AudioClip[] attackVoiceLines; // ✅ NEW: Attack voice lines

    [Header("Voice Settings")]
    [SerializeField, Range(0f, 1f)] private float voiceVolume = 0.5f;
    [SerializeField] private float minTalkDelay = 4f;
    [SerializeField] private float maxTalkDelay = 12f;
    [SerializeField] private float voiceCooldown = 3f;

    [Header("Movement Settings")]
    [SerializeField] private float movementFadeSpeed = 5f;
    [SerializeField] private float minLandVelocity = -2f;

    private PlayerController controller;
    private float originalMovementVolume;
    private Coroutine floatCoroutine;

    private float nextTalkTime = 0f;
    private float lastVoiceTime = -10f;

    private bool wasGrounded = true;
    private bool wasDashing = false;
    private bool wasJumping = false;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        if (controller == null)
        {
            Debug.LogError("PlayerAudioController requires PlayerController component.");
            enabled = false;
            return;
        }

        SetupLoops();
        PlayerController.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDestroy()
    {
        PlayerController.OnHealthChanged -= HandleHealthChanged;
    }

    private void SetupLoops()
    {
        if (movementSource != null)
        {
            movementSource.loop = true;
            originalMovementVolume = movementSource.volume;
            movementSource.Stop();
        }

        if (windSource != null)
        {
            windSource.loop = true;
            windSource.volume = 0f;
            windSource.Stop();
        }

        floatSource?.Stop();
    }

    private void Update()
    {
        float speedX = Mathf.Abs(controller.CurrentVelocity.x);
        bool isGrounded = controller.IsGrounded;
        bool isJumping = controller.IsJumping;

        HandleMovementLoop(speedX);
        HandleWindLoop(speedX);
        HandleFloatSound();
        HandleDashSound();
        HandleJumpAndLandSound(isGrounded, isJumping);
        HandleRandomVoice(isGrounded, speedX);

        wasGrounded = isGrounded;
        wasDashing = controller.IsDashing;
        wasJumping = isJumping;
    }

    // --------------------------------------------------------
    // RANDOM SFX
    // --------------------------------------------------------
    private void PlayRandomSFX(AudioClip[] clips)
    {
        if (sfxSource == null || clips == null || clips.Length == 0) return;
        sfxSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // --------------------------------------------------------
    // VOICE SYSTEM
    // --------------------------------------------------------
    private void TryPlayVoiceLine(AudioClip[] clips)
    {
        if (sfxSource == null || clips == null || clips.Length == 0) return;
        if (Time.time - lastVoiceTime < voiceCooldown) return;

        sfxSource.PlayOneShot(clips[Random.Range(0, clips.Length)], voiceVolume);
        lastVoiceTime = Time.time;
    }

    private void HandleRandomVoice(bool grounded, float speedX)
    {
        if (Time.time < nextTalkTime) return;

        bool canTalk = grounded || speedX < 0.1f || controller.IsFloating;

        if (canTalk)
            TryPlayVoiceLine(idleVoiceLines);

        nextTalkTime = Time.time + Random.Range(minTalkDelay, maxTalkDelay);
    }

    // --------------------------------------------------------
    // JUMP & LAND
    // --------------------------------------------------------
    private void HandleJumpAndLandSound(bool isGrounded, bool isJumping)
    {
        if (isJumping && !wasJumping)
        {
            PlayRandomSFX(jumpClips);
        }

        if (isGrounded && !wasGrounded && controller.CurrentVelocity.y <= minLandVelocity)
        {
            PlayRandomSFX(landClips);
        }
    }

    // --------------------------------------------------------
    // DASH / POGO
    // --------------------------------------------------------
    private void HandleDashSound()
    {
        if (controller.IsDashing && !wasDashing)
        {
            PlayRandomSFX(dashClips);
            TryPlayVoiceLine(dashVoiceLines);
        }
    }

    public void PlayPogoSound()
    {
        PlayRandomSFX(pogoClips);
    }

    // --------------------------------------------------------
    // ATTACK / SHOOTING (NEW)
    // --------------------------------------------------------
    public void PlayAttackSound()
    {
        PlayRandomSFX(attackClips);
        TryPlayVoiceLine(attackVoiceLines);
    }

    // --------------------------------------------------------
    // MOVEMENT LOOP
    // --------------------------------------------------------
    private void HandleMovementLoop(float speed)
    {
        if (movementSource == null) return;

        bool shouldPlay = speed > 0.1f && controller.IsGrounded;

        if (shouldPlay)
        {
            if (!movementSource.isPlaying)
            {
                movementSource.volume = 0f;
                movementSource.Play();
            }

            movementSource.volume = Mathf.Lerp(movementSource.volume,
                originalMovementVolume, Time.deltaTime * movementFadeSpeed);
        }
        else if (movementSource.isPlaying)
        {
            movementSource.volume = Mathf.Lerp(movementSource.volume,
                0f, Time.deltaTime * movementFadeSpeed);

            if (movementSource.volume < 0.01f)
                movementSource.Stop();
        }
    }

    // --------------------------------------------------------
    // WIND LOOP
    // --------------------------------------------------------
    private void HandleWindLoop(float speed)
    {
        if (windSource == null) return;

        const float threshold = 10f;
        const float maxSpeed = 20f;
        const float fadeSpeed = 5f;

        if (speed > threshold)
        {
            if (!windSource.isPlaying)
            {
                windSource.volume = 0f;
                windSource.Play();
            }

            float targetVol = Mathf.Clamp01((speed - threshold) / (maxSpeed - threshold));
            windSource.volume = Mathf.Lerp(windSource.volume, targetVol, Time.deltaTime * fadeSpeed);
        }
        else if (windSource.isPlaying)
        {
            windSource.volume = Mathf.Lerp(windSource.volume, 0f, Time.deltaTime * fadeSpeed);
            if (windSource.volume < 0.01f)
                windSource.Stop();
        }
    }

    // --------------------------------------------------------
    // FLOAT SOUND
    // --------------------------------------------------------
    private void HandleFloatSound()
    {
        if (floatSource == null) return;

        if (controller.IsFloating)
        {
            if (floatCoroutine == null)
                floatCoroutine = StartCoroutine(FloatSequence());
        }
        else
        {
            if (floatCoroutine != null)
            {
                StopCoroutine(floatCoroutine);
                floatCoroutine = null;
            }
            floatSource.Stop();
        }
    }

    private IEnumerator FloatSequence()
    {
        floatSource.loop = false;
        floatSource.time = 0f;

        float intro = floatSource.clip != null ? floatSource.clip.length : 1.5f;
        floatSource.Play();

        yield return new WaitForSeconds(intro);

        while (controller.IsFloating)
        {
            floatSource.time = 0f;
            floatSource.Play();
            yield return new WaitForSeconds(floatSource.clip.length);
        }

        floatSource.Stop();
        floatCoroutine = null;
    }

    // --------------------------------------------------------
    // HIT & DEATH
    // --------------------------------------------------------
    private void HandleHealthChanged(int current, int max)
    {
        if (current <= 0)
        {
            PlaySFX(deathClip);
        }
        else if (current < max)
        {
            PlaySFX(hitClip);
        }
    }
}