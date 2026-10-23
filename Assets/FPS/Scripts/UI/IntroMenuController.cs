using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Unity.FPS.UI
{
    /// <summary>
    /// Plays the looping background video for the IntroMenu scene.
    /// </summary>
    public class IntroMenuController : MonoBehaviour
    {
        [SerializeField] private RawImage m_VideoBackground;

        private const string VideoFileName = "MenuIniciaL01.mp4";
        private const string BackgroundObjectName = "BackgroundImage";
        private const string VideoSurfaceObjectName = "IntroMenuVideoBackground";

        private AspectRatioFitter m_VideoAspectFitter;
        private VideoPlayer m_VideoPlayer;

        private void Start()
        {
            ConfigureBackgroundVideo();
        }

        private void OnDestroy()
        {
            if (m_VideoPlayer == null)
                return;

            m_VideoPlayer.prepareCompleted -= OnVideoPrepared;
            m_VideoPlayer.frameReady -= OnVideoFrameReady;
            m_VideoPlayer.errorReceived -= OnVideoError;
            m_VideoPlayer.Stop();
        }

        private void ConfigureBackgroundVideo()
        {
            Canvas menuCanvas = GetComponentInParent<Canvas>();
            if (menuCanvas == null)
                menuCanvas = FindObjectOfType<Canvas>();

            if (menuCanvas == null)
            {
                Debug.LogWarning("IntroMenu could not find its Canvas for the background video.", this);
                return;
            }

            string videoPath = Path.Combine(Application.streamingAssetsPath, VideoFileName);
            if (!File.Exists(videoPath))
            {
                Debug.LogError($"IntroMenu background video was not found at '{videoPath}'.", this);
                return;
            }

            CreateVideoBackgroundSurface(menuCanvas);

            m_VideoPlayer = GetComponent<VideoPlayer>();
            if (m_VideoPlayer == null)
                m_VideoPlayer = gameObject.AddComponent<VideoPlayer>();

            m_VideoPlayer.playOnAwake = false;
            m_VideoPlayer.source = VideoSource.Url;
            m_VideoPlayer.url = videoPath;
            m_VideoPlayer.renderMode = VideoRenderMode.APIOnly;
            m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            m_VideoPlayer.isLooping = true;
            m_VideoPlayer.waitForFirstFrame = true;
            m_VideoPlayer.skipOnDrop = true;
            m_VideoPlayer.sendFrameReadyEvents = true;
            m_VideoPlayer.prepareCompleted += OnVideoPrepared;
            m_VideoPlayer.frameReady += OnVideoFrameReady;
            m_VideoPlayer.errorReceived += OnVideoError;
            m_VideoPlayer.Prepare();
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
            // Place the video over the still image and under the vignette and controls.
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

        private void OnVideoPrepared(VideoPlayer player)
        {
            player.prepareCompleted -= OnVideoPrepared;
            if (m_VideoBackground != null && player.texture != null)
                m_VideoBackground.texture = player.texture;

            if (m_VideoAspectFitter != null && player.height > 0)
                m_VideoAspectFitter.aspectRatio = (float)player.width / player.height;

            player.Play();
        }

        private void OnVideoFrameReady(VideoPlayer player, long frameIndex)
        {
            if (m_VideoBackground != null && m_VideoBackground.texture != player.texture)
                m_VideoBackground.texture = player.texture;
        }

        private void OnVideoError(VideoPlayer player, string message)
        {
            Debug.LogError($"IntroMenu could not play its background video: {message}", this);
        }
    }
}
