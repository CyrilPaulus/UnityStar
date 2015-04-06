using UnityEngine;
using System.Collections;
using Mp3Sharp;
using System.IO;
using System;
using System.Linq;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using FFmpeg.Wrapper;

public class SongScript : ITimeSource
{
    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    public string SongDir;
    public Difficulty CurrentDifficulty;
    public VideoPlayerMesh VideoPlayer;
    public GameObject SingNoteSprite;
    public GameObject NoteSprite;
    public GameObject RenderQuad;
    public SongData Song;
    public Text Text;
    public Text Text2;
    public Text TimeText;
    private Note curNote;
    private Line _curLine;
    private Line _nextLine;
    private List<GameObject> _noteSprites;

    public Slider Slider;
    public FrequencyExtractor FrequencyExtractor;
    private List<SingNoteInfo> _singNotes;
    private bool _wasBlank;
    // Use this for initialization
    void Start()
    {   
        if(!string.IsNullOrEmpty(ApplicationSettings.CurrentSongFile))
            Song = new SongData(ApplicationSettings.CurrentSongFile);
        else
            Song = new SongData(@"E:\UltraStar\songs\Acdc - Highway To Hell\Acdc - Highway To Hell.txt");

        VideoPlayer.SetFile(Song.GetVideoPath());
        VideoPlayer.VideoGap = Song.VideoGap;
        VideoPlayer.Reset();
                
        audio.clip = GetAudioClipFromMP3(Song.GetMP3Path());        

        audio.Play();
        VideoPlayer.Play();

        _noteSprites = new List<GameObject>();
        Slider.maxValue = audio.clip.length / 2;
        Slider.minValue = 0;
                        
        _singNotes = new List<SingNoteInfo>();
    }

    public override double GetTime()
    {
        var freq = audio.clip.frequency;
        var time = audio.timeSamples / (float)audio.clip.frequency;
        return time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.LoadLevel("SongSelection");

        var time = GetTime();

        var min = (int)(time / 60);
        var sec = (int)(time % 60);
        Slider.value = (float)time;
        TimeText.text = string.Format("{0:00}:{1:00}", min, sec);

        //Remove GAP
        
        time = time - (Song.GAP / 1000.0f);       
        var bpm = time * Song.BPM / 60.0f * 4; // Some how we have to multiply by 4

        var notes = Song.Data.Where(x => x is Note).Select(x => x as Note);

        var line = Song.Lines.FirstOrDefault(x => x.StartBeat <= bpm && x.EndBeat >= bpm);
        if (line != _curLine)
        {
            _curLine = line;
            if (_curLine != null)
            {
                int index = Song.Lines.IndexOf(_curLine);
                if (index + 1 < Song.Lines.Count)
                    _nextLine = Song.Lines[index + 1];
            }

            OnLineChanged();
        }


        if (line != null)
        {
            Text.text = line.GetText(bpm);
        }

        if (_nextLine != null)
        {
            Text2.text = _nextLine.GetText(bpm);
        }
        else
        {
            Text2.text = string.Empty;
        }

        UpdateNotes(bpm);
    }

    private void UpdateNotes(double bpm)
    {
        if (FrequencyExtractor.HasTone && _curLine!= null && _curLine.GetCurNote(bpm) != null)
        {

            var curTone = _curLine.GetCurNote(bpm).NoteCode;

            var tone = FrequencyExtractor.ToneAbs;
            //Difficulty and stuff

            while (tone - curTone > 6)
                tone = tone - 12;

            while (tone - curTone < -6)
                tone = tone + 12;

            var range = 2 - (int)this.CurrentDifficulty;

            //We correctly sung this tone !!! 
            if (Math.Abs(tone - curTone) < range)
                tone = curTone;

            int baseTone = _curLine.Notes.Select(n => n.NoteCode).Min();

            var first = _curLine.Notes.FirstOrDefault();
            var last = _curLine.Notes.LastOrDefault();

            //We can't use the start and end value of the line, as we don't want to blank to be visible
            var length = (float)(last.BeatNumber + last.BeatLength - first.BeatNumber);

            int noteIndex = tone - baseTone;
            float x = (float)((bpm - _curLine.StartBeat) / length) * DISPLAY_LENGTH + OFFSET_X;
            float y = (float)(-2.36 + 0.16 * noteIndex);



            var lastSingNote = _singNotes.LastOrDefault();
            if (lastSingNote != null && lastSingNote.Note == tone)// && !lastSingNote.Breaked)
            {
                var width = (bpm - lastSingNote.StartBeat) / length * DISPLAY_LENGTH;
                lastSingNote.NoteSprite.GetComponent<NoteSprite>().Width = (float)width;
            }
            else
            {
               // if(lastSingNote != null)
                 //   Debug.Log((bpm - lastSingNote.StartBeat) / length * DISPLAY_LENGTH);

                //if (lastSingNote == null || (bpm - lastSingNote.StartBeat) / length * DISPLAY_LENGTH < 1)
                //{
                    var singNote = new SingNoteInfo();
                    singNote.Note = tone;
                    singNote.Breaked = false;
                    singNote.StartBeat = bpm;
                    singNote.NoteSprite = GameObject.Instantiate(SingNoteSprite) as GameObject;
                    singNote.NoteSprite.transform.position = new Vector3(x, y, 0);
                    singNote.NoteSprite.GetComponent<NoteSprite>().Width = 0;
                    //Check if we are not too close
                    _singNotes.Add(singNote);
                //}
            }


            _wasBlank = false;
        }
        else
        {
            var lastSingNote = _singNotes.LastOrDefault();
            if (lastSingNote != null)
                lastSingNote.Breaked = true;
            _wasBlank = true;
        }
    }

    private const float DISPLAY_LENGTH = 11.4f;
    private const float OFFSET_X = -5.7f;
    private void OnLineChanged()
    {
        if (_curLine == null)
            return;

        foreach (var obj in _singNotes.Select(x => x.NoteSprite))
            GameObject.Destroy(obj);

        foreach (var obj in _noteSprites)
            GameObject.Destroy(obj);

        _singNotes.Clear();
        _noteSprites.Clear();

        var first = _curLine.Notes.FirstOrDefault();
        var last = _curLine.Notes.LastOrDefault();

        //We can't use the start and end value of the line, as we don't want to blank to be visible
        var length = (float)(last.BeatNumber + last.BeatLength - first.BeatNumber);

        var baseNote = _curLine.Notes.Select(x => x.NoteCode).Min();

        foreach (var note in _curLine.Notes)
        {
            int noteIndex = note.NoteCode - baseNote;
            float x = ((note.BeatNumber - _curLine.StartBeat) / length) * DISPLAY_LENGTH + OFFSET_X;
            float y = (float)(-2.36 + 0.16 * noteIndex);
            float width = (note.BeatLength / length) * DISPLAY_LENGTH;

            var noteSprite = GameObject.Instantiate(NoteSprite) as GameObject;
            _noteSprites.Add(noteSprite);
            noteSprite.transform.position = new Vector3(x, y, 0);
            noteSprite.GetComponent<NoteSprite>().Width = width;
        }

    }

    private AudioClip GetAudioClipFromMP3(string mp3)
    {
        var cur = DateTime.Now;
        var wrapper = new AudioWrapper();
        var stream = new MemoryStream();
        wrapper.Decode(mp3, stream);
                

        byte[] rawAudioData = stream.ToArray();
        float[] audioData = new float[rawAudioData.Length / 2];

        for (int i = 0; i < audioData.Length; i++)
        {
            audioData[i] = (float)(BitConverter.ToInt16(rawAudioData, i * 2)) / 32768.0f;
        }

        var rtn = AudioClip.Create(mp3, audioData.Length, 2, wrapper.Frequency, false, false);
        rtn.SetData(audioData, 0);
        var dur = DateTime.Now - cur;
        Debug.Log(dur.TotalSeconds);
        return rtn;

    }

}

class SingNoteInfo
{
    public int Note { get; set; }
    public double StartBeat { get; set; }
    public GameObject NoteSprite { get; set; }
    public bool Breaked { get; set; }
}
