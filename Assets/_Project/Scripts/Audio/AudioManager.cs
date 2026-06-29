using System;
using System.Collections;
using UnityEngine;

namespace MemoryTower
{
    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        private const string SettingsKey = "MemoryTower.Audio";
        private const int SfxPoolSize = 8;
        private const float BgmFadeDuration = 1f;

        private static AudioManager instance;

        [SerializeField] private AudioConfig config;
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.6f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

        private AudioSource bgmSource;
        private AudioSource[] sfxPool;
        private int sfxPoolIndex;
        private bool muted;
        private bool subscribed;
        private bool warnedMissingConfig;
        private Coroutine bgmFadeCoroutine;

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject audioObject = new GameObject("AudioManager");
                        instance = audioObject.AddComponent<AudioManager>();
                    }
                }

                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        public float MasterVolume
        {
            get { return masterVolume; }
        }

        public float BgmVolume
        {
            get { return bgmVolume; }
        }

        public float SfxVolume
        {
            get { return sfxVolume; }
        }

        public bool IsMuted
        {
            get { return muted; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            AudioManager ignored = Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Instance.config == null && config != null)
                {
                    Instance.enabled = false;
                    Destroy(Instance.gameObject);
                }
                else
                {
                    enabled = false;
                    Destroy(gameObject);
                    return;
                }
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDefaultConfigIfNeeded();
            InitializeSources();
            LoadSettings();
            ApplyVolumes();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnValidate()
        {
            masterVolume = Mathf.Clamp01(masterVolume);
            bgmVolume = Mathf.Clamp01(bgmVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);

            if (Application.isPlaying)
            {
                ApplyVolumes();
            }
        }

        public void PlaySfx(SfxType type)
        {
            if (!TryGetConfig(out AudioConfig audioConfig))
            {
                return;
            }

            AudioClip clip = audioConfig.GetSfx(type);
            if (clip == null || sfxPool == null || sfxPool.Length == 0)
            {
                return;
            }

            AudioSource source = sfxPool[sfxPoolIndex];
            sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Length;
            source.PlayOneShot(clip, audioConfig.GetSfxVolume(type));
        }

        public void PlayBgm(string sceneName)
        {
            if (!TryGetConfig(out AudioConfig audioConfig))
            {
                return;
            }

            AudioClip clip = audioConfig.GetBgm(sceneName);
            if (clip == null)
            {
                StopBgm();
                return;
            }

            if (bgmSource != null && bgmSource.clip == clip && bgmSource.isPlaying)
            {
                ApplyVolumes();
                return;
            }

            if (bgmFadeCoroutine != null)
            {
                StopCoroutine(bgmFadeCoroutine);
            }

            bgmFadeCoroutine = StartCoroutine(FadeBgmTo(clip));
        }

        public void StopBgm()
        {
            if (bgmFadeCoroutine != null)
            {
                StopCoroutine(bgmFadeCoroutine);
                bgmFadeCoroutine = null;
            }

            if (bgmSource == null)
            {
                return;
            }

            bgmSource.Stop();
            bgmSource.clip = null;
            bgmSource.volume = 0f;
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyVolumes();
            SaveSettings();
        }

        public void SetBgmVolume(float value)
        {
            bgmVolume = Mathf.Clamp01(value);
            ApplyVolumes();
            SaveSettings();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            ApplyVolumes();
            SaveSettings();
        }

        public void ToggleMute()
        {
            muted = !muted;
            ApplyVolumes();
            SaveSettings();
        }

        private IEnumerator FadeBgmTo(AudioClip nextClip)
        {
            if (bgmSource == null)
            {
                bgmFadeCoroutine = null;
                yield break;
            }

            if (bgmSource.isPlaying)
            {
                float startVolume = bgmSource.volume;
                float elapsed = 0f;
                while (elapsed < BgmFadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    bgmSource.volume = Mathf.Lerp(startVolume, 0f, Mathf.Clamp01(elapsed / BgmFadeDuration));
                    yield return null;
                }
            }

            bgmSource.Stop();
            bgmSource.clip = nextClip;
            bgmSource.loop = true;
            bgmSource.volume = 0f;
            bgmSource.Play();

            float fadeInElapsed = 0f;
            while (fadeInElapsed < BgmFadeDuration)
            {
                fadeInElapsed += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, GetBgmTargetVolume(), Mathf.Clamp01(fadeInElapsed / BgmFadeDuration));
                yield return null;
            }

            bgmSource.volume = GetBgmTargetVolume();
            bgmFadeCoroutine = null;
        }

        private void InitializeSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.loop = true;
                bgmSource.spatialBlend = 0f;
            }

            if (sfxPool != null && sfxPool.Length == SfxPoolSize)
            {
                return;
            }

            sfxPool = new AudioSource[SfxPoolSize];
            for (int i = 0; i < sfxPool.Length; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                sfxPool[i] = source;
            }
        }

        private void SubscribeToEvents()
        {
            if (subscribed)
            {
                return;
            }

            AudioEvents.OnSfxRequested += PlaySfx;
            AudioEvents.OnBgmRequested += PlayBgm;
            subscribed = true;
        }

        private void UnsubscribeFromEvents()
        {
            if (!subscribed)
            {
                return;
            }

            AudioEvents.OnSfxRequested -= PlaySfx;
            AudioEvents.OnBgmRequested -= PlayBgm;
            subscribed = false;
        }

        private void LoadDefaultConfigIfNeeded()
        {
            if (config != null)
            {
                return;
            }

            config = Resources.Load<AudioConfig>("AudioConfig");
            if (config == null)
            {
                config = Resources.Load<AudioConfig>("Audio/AudioConfig");
            }
        }

        private bool TryGetConfig(out AudioConfig audioConfig)
        {
            if (config == null)
            {
                LoadDefaultConfigIfNeeded();
            }

            audioConfig = config;
            if (audioConfig != null)
            {
                return true;
            }

            if (!warnedMissingConfig)
            {
                Debug.LogWarning("AudioManager has no AudioConfig assigned.");
                warnedMissingConfig = true;
            }

            return false;
        }

        private void ApplyVolumes()
        {
            if (bgmSource != null && bgmFadeCoroutine == null)
            {
                bgmSource.volume = GetBgmTargetVolume();
            }

            if (sfxPool == null)
            {
                return;
            }

            float sourceVolume = GetSfxSourceVolume();
            foreach (AudioSource source in sfxPool)
            {
                if (source != null)
                {
                    source.volume = sourceVolume;
                }
            }
        }

        private float GetBgmTargetVolume()
        {
            return muted ? 0f : masterVolume * bgmVolume;
        }

        private float GetSfxSourceVolume()
        {
            return muted ? 0f : masterVolume * sfxVolume;
        }

        private void SaveSettings()
        {
            AudioSettingsData data = new AudioSettingsData();
            data.masterVolume = masterVolume;
            data.bgmVolume = bgmVolume;
            data.sfxVolume = sfxVolume;
            data.muted = muted;

            PlayerPrefs.SetString(SettingsKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            if (!PlayerPrefs.HasKey(SettingsKey))
            {
                return;
            }

            try
            {
                AudioSettingsData data = JsonUtility.FromJson<AudioSettingsData>(PlayerPrefs.GetString(SettingsKey));
                if (data == null)
                {
                    return;
                }

                masterVolume = Mathf.Clamp01(data.masterVolume);
                bgmVolume = Mathf.Clamp01(data.bgmVolume);
                sfxVolume = Mathf.Clamp01(data.sfxVolume);
                muted = data.muted;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Audio settings load failed: " + exception.Message);
            }
        }

        [Serializable]
        private sealed class AudioSettingsData
        {
            public float masterVolume = 1f;
            public float bgmVolume = 0.6f;
            public float sfxVolume = 1f;
            public bool muted;
        }
    }
}
