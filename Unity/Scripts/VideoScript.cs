using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoMenu : MonoBehaviour {
    public VideoClip backgroundVideo;
    public RawImage videoScreen;

    void Start() {
        VideoPlayer videoPlayer = gameObject.AddComponent<VideoPlayer>();

        videoPlayer.clip = backgroundVideo;
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = true;
        
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = new RenderTexture(1920, 1080, 24);
        
        if (videoScreen != null) {
            videoScreen.texture = videoPlayer.targetTexture;
        }
        
        videoPlayer.Play();
        
        Debug.Log("Видео запущено: " + backgroundVideo.name);
    }
}