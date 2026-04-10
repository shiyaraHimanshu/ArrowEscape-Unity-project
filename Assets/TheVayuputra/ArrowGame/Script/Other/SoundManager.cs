using UnityEngine;

namespace ArrowGame
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("SFX")]
        [SerializeField] private AudioClip uiClickSfxClip;
        [SerializeField] private AudioClip arrowEscapeFailedSfx;
        [SerializeField] private AudioClip arrowEscapeSuccessSfx;
        [SerializeField] private AudioClip levelCompletedSfx;
        [SerializeField] private AudioClip levelFailedSfx;
        [SerializeField] private AudioClip collectCoinSfx;

        [Header("Volume")]
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Listen to GameData changes
            GameData.MusicOn.AddListener(OnMusicToggle);

            ApplyInitialSettings();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                GameData.MusicOn.RemoveListener(OnMusicToggle);
            }
        }

        // ---------------- MUSIC ----------------
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (!GameData.MusicOn.Value || clip == null)
                return;

            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        void OnMusicToggle(bool isOn)
        {
            if (!isOn)
                musicSource.Stop();
            else if (musicSource.clip != null)
                musicSource.Play();
        }

        // ---------------- SFX ----------------
        public void PlaySFX(AudioClip clip)
        {
            if (!GameData.SfxOn.Value || clip == null)
                return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
        public void PlayUIClickSFX()
        {
            PlaySFX(uiClickSfxClip);
        }
        // ---------------- GAMEPLAY SFX ----------------

        public void PlayArrowEscapeFailedSFX()
        {
            PlaySFX(arrowEscapeFailedSfx);
            Vibrate();
        }

        public void PlayArrowEscapeSuccessSFX()
        {
            PlaySFX(arrowEscapeSuccessSfx);
        }

        public void PlayLevelCompletedSFX()
        {
            PlaySFX(levelCompletedSfx);
        }
        public void PlayCollectCoinSFX()
        {
            PlaySFX(collectCoinSfx);
        }
        public void PlayLevelFailedSFX()
        {
            PlaySFX(levelFailedSfx);
            Vibrate();
        }

        // ---------------- VIBRATION ----------------
        public void Vibrate()
        {
            if (!GameData.VibrationOn.Value)
                return;

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        void ApplyInitialSettings()
        {
            musicSource.loop=true;
            musicSource.volume = musicVolume;
            sfxSource.volume = sfxVolume;

            if (!GameData.MusicOn.Value)
                musicSource.Stop();
        }
    }
}
