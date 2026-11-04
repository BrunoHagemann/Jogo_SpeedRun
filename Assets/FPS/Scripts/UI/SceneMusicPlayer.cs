using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.FPS.UI
{
    /// <summary>
    /// Plays music only in scenes that have an assigned background track.
    /// </summary>
    public class SceneMusicPlayer : MonoBehaviour
    {
        private const string IntroMenuSceneName = "IntroMenu";
        private const string CitySceneName = "Cidade";
        private const string CityAudioObjectName = "GerenciadorDeAudio";
        private const string MenuMusicResourcePath = "Musicas/Menu";

        private static SceneMusicPlayer s_Instance;
        private static float s_MusicVolume = 1f;

        private AudioSource m_AudioSource;
        private AudioSource m_CityAudioSource;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateMusicPlayer()
        {
            if (s_Instance != null)
                return;

            GameObject musicObject = new GameObject(nameof(SceneMusicPlayer));
            DontDestroyOnLoad(musicObject);
            musicObject.AddComponent<SceneMusicPlayer>();
        }

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.playOnAwake = false;
            m_AudioSource.loop = true;
            m_AudioSource.spatialBlend = 0f;
            m_AudioSource.volume = s_MusicVolume;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            PlayMusicForScene(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
                s_Instance = null;
        }

        public static void StopMusic()
        {
            if (s_Instance == null)
                return;

            if (s_Instance.m_AudioSource != null)
                s_Instance.m_AudioSource.Stop();

            if (s_Instance.m_CityAudioSource != null)
                s_Instance.m_CityAudioSource.Stop();
        }

        public static void SetMusicVolume(float volume)
        {
            s_MusicVolume = Mathf.Clamp01(volume);

            if (s_Instance == null)
                return;

            if (s_Instance.m_AudioSource != null)
                s_Instance.m_AudioSource.volume = s_MusicVolume;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayMusicForScene(scene);
        }

        private void PlayMusicForScene(Scene scene)
        {
            if (scene.name == CitySceneName)
            {
                PlayCityMusic();
                return;
            }

            if (m_CityAudioSource != null)
                m_CityAudioSource.Stop();
            m_CityAudioSource = null;

            string resourcePath = GetMusicResourcePath(scene.name);
            if (string.IsNullOrEmpty(resourcePath))
            {
                m_AudioSource.Stop();
                m_AudioSource.clip = null;
                return;
            }

            AudioClip music = Resources.Load<AudioClip>(resourcePath);
            if (music == null)
            {
                Debug.LogError($"Scene music was not found at Resources/{resourcePath}.", this);
                m_AudioSource.Stop();
                m_AudioSource.clip = null;
                return;
            }

            if (m_AudioSource.isPlaying && m_AudioSource.clip == music)
                return;

            m_AudioSource.Stop();
            m_AudioSource.clip = music;
            m_AudioSource.loop = true;
            m_AudioSource.Play();
        }

        private void PlayCityMusic()
        {
            m_AudioSource.Stop();
            m_AudioSource.clip = null;

            GameObject audioObject = GameObject.Find(CityAudioObjectName);
            AudioSource cityAudioSource = audioObject != null
                ? audioObject.GetComponent<AudioSource>()
                : null;

            if (cityAudioSource == null || cityAudioSource.clip == null)
            {
                Debug.LogError($"City music AudioSource was not found on '{CityAudioObjectName}'.", this);
                m_CityAudioSource = null;
                return;
            }

            m_CityAudioSource = cityAudioSource;
            m_CityAudioSource.loop = true;
            m_CityAudioSource.spatialBlend = 0f;

            if (!m_CityAudioSource.isPlaying)
                m_CityAudioSource.Play();
        }

        private static string GetMusicResourcePath(string sceneName)
        {
            if (sceneName == IntroMenuSceneName)
                return MenuMusicResourcePath;

            return null;
        }
    }
}
