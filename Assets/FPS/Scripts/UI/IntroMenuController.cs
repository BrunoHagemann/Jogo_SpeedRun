using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Unity.FPS.UI
{
    /// <summary>
    /// Plays the looping IntroMenu background and the Play cinematic.
    /// </summary>
    public class IntroMenuController : MonoBehaviour
    {
        [SerializeField] private RawImage m_VideoBackground;

        private const string BackgroundVideoFileName = "MenuIniciaL01.mp4";
        private const string PlayVideoFileName = "Play.mp4";
        private const string MainSceneName = "Cidade";
        private const string BackgroundObjectName = "BackgroundImage";
        private const string VideoSurfaceObjectName = "IntroMenuVideoBackground";
        private const string PlayButtonObjectName = "StartButton";
        private const float PlayVideoPrepareTimeout = 10f;

        private AspectRatioFitter m_VideoAspectFitter;
        private VideoPlayer m_VideoPlayer;
        private Button m_PlayButton;
        private Canvas m_MenuCanvas;
        private bool m_PlaySequenceStarted;
        private bool m_MainSceneLoading;

        private void Start()
        {
            m_MenuCanvas = GetComponentInParent<Canvas>();
            if (m_MenuCanvas == null)
                m_MenuCanvas = FindObjectOfType<Canvas>();

            ConfigurePlayButton();
            ConfigureBackgroundVideo();
        }

        private void OnDestroy()
        {
            if (m_PlayButton != null)
                m_PlayButton.onClick.RemoveListener(BeginPlayCinematic);

            if (m_VideoPlayer == null)
                return;

            m_VideoPlayer.prepareCompleted -= OnBackgroundVideoPrepared;
            m_VideoPlayer.prepareCompleted -= OnPlayVideoPrepared;
            m_VideoPlayer.frameReady -= OnVideoFrameReady;
            m_VideoPlayer.loopPointReached -= OnPlayVideoFinished;
            m_VideoPlayer.errorReceived -= OnBackgroundVideoError;
            m_VideoPlayer.errorReceived -= OnPlayVideoError;
            m_VideoPlayer.Stop();
        }

        private void ConfigurePlayButton()
        {
            GameObject buttonObject = GameObject.Find(PlayButtonObjectName);
            if (buttonObject == null)
            {
                Debug.LogWarning("IntroMenu could not find its Play button.", this);
                return;
            }

            m_PlayButton = buttonObject.GetComponent<Button>();
            if (m_PlayButton == null)
            {
                Debug.LogWarning("IntroMenu's Play object has no Button component.", this);
                return;
            }

            // The scene's legacy component loads MainScene immediately on submit.
            LoadSceneButton legacyLoader = buttonObject.GetComponent<LoadSceneButton>();
            if (legacyLoader != null)
                legacyLoader.enabled = false;

            m_PlayButton.onClick.AddListener(BeginPlayCinematic);
        }

        private void ConfigureBackgroundVideo()
        {
            if (m_MenuCanvas == null)
            {
                Debug.LogWarning("IntroMenu could not find its Canvas for the background video.", this);
                return;
            }

            string videoPath = Path.Combine(Application.streamingAssetsPath, BackgroundVideoFileName);
            if (!File.Exists(videoPath))
            {
                Debug.LogError($"IntroMenu background video was not found at '{videoPath}'.", this);
                return;
            }

            CreateVideoBackgroundSurface(m_MenuCanvas);
            EnsureVideoPlayer();
            m_VideoPlayer.source = VideoSource.Url;
            m_VideoPlayer.url = videoPath;
            m_VideoPlayer.renderMode = VideoRenderMode.APIOnly;
            m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            m_VideoPlayer.isLooping = true;
            m_VideoPlayer.waitForFirstFrame = true;
            m_VideoPlayer.skipOnDrop = true;
            m_VideoPlayer.sendFrameReadyEvents = true;
            m_VideoPlayer.prepareCompleted += OnBackgroundVideoPrepared;
            m_VideoPlayer.frameReady += OnVideoFrameReady;
            m_VideoPlayer.errorReceived += OnBackgroundVideoError;
            m_VideoPlayer.Prepare();
        }

        private void EnsureVideoPlayer()
        {
            m_VideoPlayer = GetComponent<VideoPlayer>();
            if (m_VideoPlayer == null)
                m_VideoPlayer = gameObject.AddComponent<VideoPlayer>();

            m_VideoPlayer.playOnAwake = false;
        }

        private void CreateVideoBackgroundSurface(Canvas canvas)
        {
            Transform background = canvas.transform.Find(BackgroundObjectName);
            if (background == null)
                background = canvas.transform;

            Transform existingSurface = m_VideoBackground != null
                ? m_VideoBackground.transform
                : background.Find(VideoSurfaceObjectName);
            GameObject backgroundObject;
            if (existingSurface != null)
            {
                backgroundObject = existingSurface.gameObject;
            }
            else
            {
                backgroundObject = new GameObject(
                    VideoSurfaceObjectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(RawImage),
                    typeof(AspectRatioFitter));
            }

            RectTransform rectTransform = backgroundObject.GetComponent<RectTransform>();
            rectTransform.SetParent(background, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.SetAsFirstSibling();

            m_VideoBackground = backgroundObject.GetComponent<RawImage>();
            m_VideoBackground.color = Color.white;
            m_VideoBackground.raycastTarget = false;

            m_VideoAspectFitter = backgroundObject.GetComponent<AspectRatioFitter>();
            if (m_VideoAspectFitter == null)
                m_VideoAspectFitter = backgroundObject.AddComponent<AspectRatioFitter>();
            m_VideoAspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            m_VideoAspectFitter.aspectRatio = 16f / 9f;
        }

        private void BeginPlayCinematic()
        {
            if (m_PlaySequenceStarted || m_MainSceneLoading)
                return;

            m_PlaySequenceStarted = true;
            if (m_PlayButton != null)
                m_PlayButton.interactable = false;

            if (m_VideoPlayer != null)
            {
                m_VideoPlayer.prepareCompleted -= OnBackgroundVideoPrepared;
                m_VideoPlayer.errorReceived -= OnBackgroundVideoError;
                m_VideoPlayer.loopPointReached -= OnPlayVideoFinished;
                m_VideoPlayer.Stop();
            }

            if (m_VideoBackground != null && m_MenuCanvas != null)
            {
                RectTransform rectTransform = m_VideoBackground.rectTransform;
                rectTransform.SetParent(m_MenuCanvas.transform, false);
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.SetAsLastSibling();
            }

            string videoPath = Path.Combine(Application.streamingAssetsPath, PlayVideoFileName);
            if (!File.Exists(videoPath))
            {
                Debug.LogError($"Play cinematic was not found at '{videoPath}'. Loading MainScene.", this);
                LoadMainScene();
                return;
            }

            EnsureVideoPlayer();
            m_VideoPlayer.source = VideoSource.Url;
            m_VideoPlayer.url = videoPath;
            m_VideoPlayer.renderMode = VideoRenderMode.APIOnly;
            m_VideoPlayer.isLooping = false;
            m_VideoPlayer.waitForFirstFrame = true;
            // Direct output sends the video's audio tracks straight to the active AudioListener.
            m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            m_VideoPlayer.prepareCompleted += OnPlayVideoPrepared;
            m_VideoPlayer.errorReceived += OnPlayVideoError;
            m_VideoPlayer.loopPointReached += OnPlayVideoFinished;
            m_VideoPlayer.Prepare();
            StartCoroutine(LoadMainSceneIfPrepareTimesOut());
        }

        private IEnumerator LoadMainSceneIfPrepareTimesOut()
        {
            yield return new WaitForSecondsRealtime(PlayVideoPrepareTimeout);
            if (m_PlaySequenceStarted && !m_MainSceneLoading
                && m_VideoPlayer != null && !m_VideoPlayer.isPlaying)
            {
                Debug.LogError("Play cinematic did not start before the timeout. Loading MainScene.", this);
                LoadMainScene();
            }
        }

        private void OnBackgroundVideoPrepared(VideoPlayer player)
        {
            player.prepareCompleted -= OnBackgroundVideoPrepared;
            AssignVideoTexture(player);
            player.Play();
        }

        private void OnPlayVideoPrepared(VideoPlayer player)
        {
            player.prepareCompleted -= OnPlayVideoPrepared;
            AssignVideoTexture(player);

            if (player.height > 0 && m_VideoAspectFitter != null)
                m_VideoAspectFitter.aspectRatio = (float)player.width / player.height;

            player.controlledAudioTrackCount = player.audioTrackCount;
            for (ushort track = 0; track < player.audioTrackCount; track++)
            {
                player.EnableAudioTrack(track, true);
            }

            if (player.audioTrackCount == 0)
                Debug.LogWarning("Play cinematic has no audio tracks to play.", this);

            player.Play();
        }

        private void AssignVideoTexture(VideoPlayer player)
        {
            if (m_VideoBackground != null && player.texture != null)
                m_VideoBackground.texture = player.texture;

            if (player.height > 0 && m_VideoAspectFitter != null)
                m_VideoAspectFitter.aspectRatio = (float)player.width / player.height;
        }

        private void OnVideoFrameReady(VideoPlayer player, long frameIndex)
        {
            if (m_VideoBackground != null && m_VideoBackground.texture != player.texture)
                m_VideoBackground.texture = player.texture;
        }

        private void OnBackgroundVideoError(VideoPlayer player, string message)
        {
            Debug.LogError($"IntroMenu could not play its background video: {message}", this);
        }

        private void OnPlayVideoError(VideoPlayer player, string message)
        {
            Debug.LogError($"Play cinematic failed: {message}. Loading MainScene.", this);
            LoadMainScene();
        }

        private void OnPlayVideoFinished(VideoPlayer player)
        {
            LoadMainScene();
        }

        private void LoadMainScene()
        {
            if (m_MainSceneLoading)
                return;

            m_MainSceneLoading = true;
            if (m_VideoPlayer != null)
            {
                m_VideoPlayer.prepareCompleted -= OnPlayVideoPrepared;
                m_VideoPlayer.errorReceived -= OnPlayVideoError;
                m_VideoPlayer.loopPointReached -= OnPlayVideoFinished;
                m_VideoPlayer.Stop();
            }

            SceneManager.LoadScene(MainSceneName);
        }
    }
}
