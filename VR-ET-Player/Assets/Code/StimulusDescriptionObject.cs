

using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class StimulusDescriptionObject
{
    public RenderTexture rt_material;
    public RenderTexture rt_skybox;


    public Sprite image;

    public VideoPlayer vp;
    public string path;
    public StimulusType stimulusType;
    public Dimension dimension;
    public int resolution_x, resolution_y;


    public StimulusDescriptionObject(StimulusType stimulusType, Dimension dimension, int resolution_x, int resolution_y, string path)
    {

        this.stimulusType = stimulusType;
        this.dimension = dimension;
        this.resolution_x = resolution_x;
        this.resolution_y = resolution_y;
        this.path = path;

    }

    internal void setPlayer(Dimension dimension, GameObject gameObject)
    {
        if (dimension == Dimension.THREE_D && stimulusType == StimulusType.VIDEO)
        {
            // 3D
            this.vp = gameObject.AddComponent<VideoPlayer>();
            rt_material = Resources.Load("rt_3D360") as RenderTexture;

            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.targetTexture = rt_material;

            //Disable Play on Awake for both Video and Audio
            vp.playOnAwake = false;

            //We want to play from video clip not from url
            vp.source = VideoSource.Url;

            //Set Audio Output to AudioSource
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;

            //Assign the Audio from Video to AudioSource to be played
            vp.EnableAudioTrack(0, true);
            //videoPlayer.SetTargetAudioSource(0, audioSource);

            //Set video Clip To Play 
            vp.url = this.path;
        }
        else
        {
            // 2D
            this.vp = gameObject.AddComponent<VideoPlayer>();
            rt_material = Resources.Load("rt_2D360") as RenderTexture;

            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.targetTexture = rt_material;

            //Disable Play on Awake for both Video and Audio
            vp.playOnAwake = false;

            //We want to play from video clip not from url
            vp.source = VideoSource.Url;

            //Set Audio Output to AudioSource
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;

            //Assign the Audio from Video to AudioSource to be played
            vp.EnableAudioTrack(0, true);
            //videoPlayer.SetTargetAudioSource(0, audioSource);

            //Set video Clip To Play 
            vp.url = this.path;
        }
    }

    internal void setSprite(Dimension dimension, GameObject gameObject)
    {
        //Sprite sprite;
        Texture2D spriteTexture = LoadTexture(this.path);
        image = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), 100.0f, 0, SpriteMeshType.Tight);
    }


    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);            // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))          // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                       // If data = readable -> return texture
        }
        else
        {
            Debug.Log("Image not found");
        }
        return null;                     // Return null if load failed
    }

}
