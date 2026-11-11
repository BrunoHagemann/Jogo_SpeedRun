using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Unity.FPS.UI
{
    /// <summary>
    /// Plays a looping, muted video behind a scene's UI.
    /// </summary>
    public class SceneBackgroundVideo : MonoBehaviour
    {
        [SerializeField] private string m_VideoFileName;
        [SerializeField] private RawImage m_VideoBackground;

        private const string BackgroundObjectName = "BackgroundImage";
        private const string VideoSurfaceObjectName = "SceneVideoBackground";

        private AspectRatioFitter m_AspectFitter;
        private VideoPlayer m_VideoPlayer;

        private void Start()
        {
            if (string.IsNullOrWhiteSpace(m_VideoFileName))
            {
                Debug.LogError("A background video file name must be assigned.", this);
                return;
            }

            string videoPath = Path.Combine(Application.streamingAssetsPath, m_VideoFileName);
            if (!File.Exists(videoPath))
            {
                Debug.LogError($"Scene background video was not found at '{videoPath}'.", this);
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Scene background video could not find its Canvas.", this);
                return;
            }

            CreateVideoSurface(canvas);
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

        private void OnDestroy()
        {
            if (m_VideoPlayer == null)
                return;

            m_VideoPlayer.prepareCompleted -= OnVideoPrepared;
            m_VideoPlayer.frameReady -= OnVideoFrameReady;
            m_VideoPlayer.errorReceived -= OnVideoError;
            m_VideoPlayer.Stop();
        }

        private void CreateVideoSurface(Canvas canvas)
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

            m_AspectFitter = backgroundObject.GetComponent<AspectRatioFitter>();
            if (m_AspectFitter == null)
                m_AspectFitter = backgroundObject.AddComponent<AspectRatioFitter>();
            m_AspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            m_AspectFitter.aspectRatio = 16f / 9f;
        }

        private void OnVideoPrepared(VideoPlayer player)
        {
            UpdateVideoSurface(player);
            player.Play();
        }

        private void OnVideoFrameReady(VideoPlayer player, long frameIndex)
        {
            if (m_VideoBackground != null && m_VideoBackground.texture != player.texture)
                m_VideoBackground.texture = player.texture;
        }

        private void UpdateVideoSurface(VideoPlayer player)
        {
            if (m_VideoBackground != null && player.texture != null)
                m_VideoBackground.texture = player.texture;

            if (player.height > 0 && m_AspectFitter != null)
                m_AspectFitter.aspectRatio = (float)player.width / player.height;
        }

        private void OnVideoError(VideoPlayer player, string message)
        {
            Debug.LogError($"Scene background video failed: {message}", this);
        }
    }
}
