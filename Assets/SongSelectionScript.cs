using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SongSelectionScript : MonoBehaviour {

    public VideoPlayerMesh Mesh;
    public string WorkingDir;
    public SimpleTimeSource TimeSource;
    public SongInfoScript SongInfo;


    private List<SongData> _songs = new List<SongData>();
    private int _curSongIndex = 0;

	// Use this for initialization
	void Start () {

        LoadSongs();
        ChangeSong();
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            PrevSong();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            NextSong();	

        if (Input.GetKeyDown(KeyCode.Return))
            SelectSong();
	}

    private void LoadSongs() 
    {

        foreach (var dir in Directory.GetDirectories(WorkingDir))
        {
            foreach(var file in Directory.GetFiles(dir)) {
                if (!file.EndsWith(".txt"))
                    continue;
                try {
                var sg = new SongData(file);
                _songs.Add(sg);
                }catch {
                    Debug.LogWarning(string.Format("File {0} is an invalid song file", file));
                }
            }
        }

        _songs = _songs.OrderBy(x => x.Artist).ThenBy(x => x.Title).ToList();

        SongInfo.SetCount(_songs.Count);

        if(string.IsNullOrEmpty(ApplicationSettings.CurrentSongFile))
            _curSongIndex = 0;
        else {
            var song = _songs.FirstOrDefault(x => x.FileName == ApplicationSettings.CurrentSongFile);
                _curSongIndex = _songs.IndexOf(song);
                }
    }

    private void ChangeSong() {
        var curSong = _songs [_curSongIndex];
        SongInfo.SetSong(curSong, _curSongIndex);
        TimeSource.Reset();
        Mesh.SetFile(curSong.GetVideoPath());
        Mesh.Play();
    }

    private void NextSong() {
        _curSongIndex = (_curSongIndex + 1) % _songs.Count;
        ChangeSong();
    }

    private void PrevSong() {
        if (_curSongIndex == 0)
            _curSongIndex = _songs.Count - 1;
        else
            _curSongIndex = (_curSongIndex - 1) % _songs.Count;
        ChangeSong();
    }

    private void SelectSong() {
        var curSong = _songs [_curSongIndex];
        ApplicationSettings.CurrentSongFile = curSong.FileName;
        Application.LoadLevel("Song");
    }
}
