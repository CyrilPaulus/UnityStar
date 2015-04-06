using UnityEngine;
using System.Collections;
using FFmpeg.Wrapper;
using UnityEngine.UI;

[RequireComponent(typeof(MeshRenderer))]
public class VideoPlayerMesh : MonoBehaviour {
    
    private VideoWrapper _wrapper;
    private bool _playing;
    private double _playtime;
    private byte[] _buffer;
    private Texture2D _texture;
    
    public double VideoGap;
    public ITimeSource TimeSource;

    public bool Playing
    {
        get
        {
            return _playing;
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (_playing)
        {
            _wrapper.ReadFrame(VideoGap + TimeSource.GetTime(), _buffer);
            _texture.LoadRawTextureData(_buffer);
            _texture.Apply();
        }

	}

    public void SetFile(string filepath)
    {
        Reset();
        if (_wrapper != null)
            _wrapper.Dispose();

        _wrapper = new VideoWrapper();
        _wrapper.Open(filepath);

        _buffer = new byte[_wrapper.Width * _wrapper.Height * 3];
        _texture = new Texture2D(_wrapper.Width, _wrapper.Height, TextureFormat.RGB24, false);

        GetComponent<Renderer>().material.mainTexture = _texture;
        var ri = GetComponent<RawImage>();
        if (ri != null)
        {
            ri.texture = _texture;
        }
    }

    public void Reset()
    {
        _playing = false;
        _playtime = VideoGap;
        if (_wrapper != null)
            _wrapper.Position = VideoGap > 0 ? VideoGap : 0;
    }

    public void Play()
    {
        _playing = true;
    }

    public void Pause()
    {
        _playing = false;
    }

    public void SetTime(double sec)
    {
        _playtime = VideoGap + sec;
    }
}
