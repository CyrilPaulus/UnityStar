using FFmpeg.Wrapper;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityStar.Assets.Timing;
using UnityStar.Assets.Video;

namespace UnityStar.Assets.Song
{
    public class SongSelectionScript : ITimeSource
    {
        public VideoPlayerMesh Mesh;
        public string WorkingDir;
        public SongInfoScript SongInfo;

        private List<SongData> _songs = new List<SongData>();
        private int _curSongIndex = 0;
        private AudioWrapper _wrapper;

        #region implemented abstract members of ITimeSource

        public override double GetTime()
        {
            var freq = GetComponent<AudioSource>().clip.frequency;
            var time = GetComponent<AudioSource>().timeSamples / (float)freq;
            return time;
        }

        #endregion implemented abstract members of ITimeSource

        // Use this for initialization
        private void Start()
        {
            //HardCode for debug only
            WorkingDir = @"E:\UltraStar\songs\";
            if (!Directory.Exists(WorkingDir))
                WorkingDir = Application.dataPath + "/../songs/";

            LoadSongs();
            ChangeSong();
        }

        // Update is called once per frame
        private void Update()
        {
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
                foreach (var file in Directory.GetFiles(dir))
                {
                    if (!file.EndsWith(".txt"))
                        continue;
                    try
                    {
                        var sg = new SongData(file);
                        _songs.Add(sg);
                    }
                    catch
                    {
                        Debug.LogWarning(string.Format("File {0} is an invalid song file", file));
                    }
                }
            }

            _songs = _songs.OrderBy(x => x.Artist).ThenBy(x => x.Title).ToList();

            SongInfo.SetCount(_songs.Count);

            if (string.IsNullOrEmpty(ApplicationSettings.CurrentSongFile))
                _curSongIndex = 0;
            else
            {
                var song = _songs.FirstOrDefault(x => x.FileName == ApplicationSettings.CurrentSongFile);
                _curSongIndex = _songs.IndexOf(song);
            }
        }

        private void ChangeSong()
        {
            var curSong = _songs[_curSongIndex];
            SongInfo.SetSong(curSong, _curSongIndex);
            Mesh.SetFile(curSong.GetVideoPath());
            Mesh.Play();
            GetComponent<AudioSource>().clip = GetAudioClipFromMP3(curSong.GetMP3Path());
            GetComponent<AudioSource>().Play();
        }

        private void NextSong()
        {
            _curSongIndex = (_curSongIndex + 1) % _songs.Count;
            ChangeSong();
        }

        private void PrevSong()
        {
            if (_curSongIndex == 0)
                _curSongIndex = _songs.Count - 1;
            else
                _curSongIndex = (_curSongIndex - 1) % _songs.Count;
            ChangeSong();
        }

        private void SelectSong()
        {
            var curSong = _songs[_curSongIndex];
            ApplicationSettings.CurrentSongFile = curSong.FileName;
            Application.LoadLevel("Song");
        }

        private AudioClip GetAudioClipFromMP3(string mp3)
        {
            _wrapper = new AudioWrapper();
            _wrapper.Open(mp3);

            var rtn = AudioClip.Create(mp3, (int)_wrapper.Samples, 2, _wrapper.Frequency, false, OnAudioRead);
            return rtn;
        }

        private void OnAudioRead(float[] data)
        {
            _wrapper.Read(data);
        }
    }
}