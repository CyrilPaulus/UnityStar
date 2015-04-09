using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using System.Linq;

public class SongData  {

    public string Title { get; set; }
    public string Artist {get; set;}
    public string Mp3File {get; set;}
    public string Cover {get; set;}
    public string Background {get; set;}
    public string Video {get; set;}
    public float VideoGap {get; set;}
    public string Edition {get; set;}
    public string FileName {get; set;}
    public string Year { get; set; }
    //Rate at which the lyrics are updated
    public float BPM { get; set; }

    //Time in milliseconds before the lyrics start
    public float GAP {get; set;}

    public List<ISongData> Data {get; private set;}
    public List<Line> Lines { get; private set; }

    public string GetMP3Path() {
        var dir = Path.GetDirectoryName(FileName);
        return Path.Combine(dir, Mp3File);
    }

    public string GetVideoPath()
    {
        var dir = Path.GetDirectoryName(FileName);
        return Path.Combine(dir, Video);
    }

    public SongData(string filename) 
    {
        FileName = filename;
        Data = new List<ISongData>();
        using(var sr = new StreamReader(filename)) {

            while(!sr.EndOfStream) 
            {
                var line = sr.ReadLine();

                if(line.StartsWith("#")) 
                    ParseMetaData(line.Substring(1));
                else if (line.StartsWith("E"))
                    break;
                else
                    ParseSongData(line);

            }
        }
        ExtractLines();
    }

    void ExtractLines()
    {
        Lines = new List<Line>();

        var curLine = new Line();
        curLine.StartBeat = 0;
        foreach (var data in Data)
        {
            if(data is Note) {
                var note = data as Note;
                curLine.Notes.Add(note);                
                curLine.Text += note.Syllab;
                if(note.Word)
                    curLine.Text += " ";
                if(curLine.StartBeat == -1)
                    curLine.StartBeat = note.BeatNumber;
                curLine.EndBeat = note.BeatNumber + note.BeatLength;
            }

            if(data is LineBreak) {
                var lineBreak = data as LineBreak;
                curLine.EndBeat = lineBreak.PrevLineBeat;
                Lines.Add(curLine);
                curLine = new Line();
                curLine.StartBeat = -1;
            }
        }

        if (!Lines.Contains(curLine))
        {
            Lines.Add(curLine);
        }

    }

    private void ParseSongData(string line) {
        if (line.StartsWith("-"))
            ParseLineBreak(line.Substring(1));
        else
            ParseNote(line);
    }

    private void ParseLineBreak(string line) {
        var lineBreak = new LineBreak();
        var data = line.Split(new string[]{" "}, System.StringSplitOptions.RemoveEmptyEntries);
        lineBreak.PrevLineBeat = int.Parse(data [0]);
        if (data.Length > 1)
            lineBreak.NextLineBeat = int.Parse(data [1]);

        Data.Add(lineBreak);
    }

    private void ParseNote(string line) {

        var note = new Note();
        if (line.StartsWith(":"))
            note.Type = NoteType.Regular;
        else if (line.StartsWith("*"))
            note.Type = NoteType.Golden;
        else if (line.StartsWith("F"))
            note.Type = NoteType.FreeStyle;

        var newline = line.Substring(2);

        if (newline.EndsWith(" "))
            note.Word = true;

        var data = newline.Split( new char[]{' '}, 4);
        note.BeatNumber = int.Parse(data [0]);
        note.BeatLength = int.Parse(data [1]);
        note.NoteCode = int.Parse(data [2]);

        if(data.Length > 3)
            note.Syllab = data [3];

        Data.Add(note);

    }

    private void ParseMetaData(string line) {        
        var data = line.Split(':');
        switch(data[0].ToUpper()) {
        case "TITLE":
            this.Title = data[1];
            break;
        case "ARTIST":
            this.Artist = data[1];
            break;
        case "MP3":
            this.Mp3File = data[1];
            break;
        case "COVER":
            this.Cover = data[1];
            break;
        case "BACKGROUND":
            this.Background = data[1];
            break;
        case "VIDEO":
            this.Video = data[1];
            break;
            
        case "VIDEOGAP":
            this.VideoGap = ParseFloat(data[1]);
            break;
            
        case "EDITION":
            this.Edition = data[1];
            break;
            
        case "BPM":           
            this.BPM = ParseFloat(data[1]);
            break;
            
        case "GAP":
            this.GAP = ParseFloat(data[1]);
            break;

        case "YEAR":
            this.Year = data[1];
            break;
        }
    }

    private float ParseFloat(string data)
    {
        data = data.Replace(',', '.');
        return float.Parse(data, CultureInfo.InvariantCulture);
    }
}


public interface ISongData {}

public enum NoteType {
    Regular,
    Golden,
    FreeStyle
}

public class Note : ISongData {

    public NoteType Type {get; set;}

    public int ScoreValue {
        get {
            if (Type == NoteType.FreeStyle)
                    return 0;
            else if (Type == NoteType.Regular)
                    return 1;
            else if (Type == NoteType.Golden)
                    return 2;
            return 0;
        }
    }

    public bool Word {get; set;}

    //When does this appears
    public int BeatNumber {get; set;}
    //Duration in beat
    public int BeatLength {get; set;}
    //Note to play
    public int NoteCode {get; set;}

    //Syllab, if it ends with a space, new word !
    public string Syllab {get; set;}

}


public class LineBreak : ISongData {

    //When the last line should dissapear
    public int PrevLineBeat {get; set;}

    //When the next line appears
    public int NextLineBeat {get; set;}
}

public class Line {
    public Line()
    {
        Notes = new List<Note>();
    }
    public int StartBeat { get; set; }
    public int EndBeat {get; set;}
    public string Text { get; set; }
    public List<Note> Notes { get; set; }

    public string GetText(double bpm)
    {
        var sb = new StringBuilder();
        foreach (var n in Notes)
        {
            if (n.BeatNumber <= bpm &&  bpm <= n.BeatNumber + n.BeatLength)
                sb.Append("<color=#013ADF>" + n.Syllab + "</color>");
            else
                sb.Append(n.Syllab);
        }

        return sb.ToString();
    }

    internal Note GetCurNote(double bpm)
    {
        return Notes.FirstOrDefault(n => n.BeatNumber <= bpm && bpm <= n.BeatNumber + n.BeatLength);
    }

    public int ScoreValue {
        get 
        {
            return Notes.Sum(x => x.BeatLength * x.ScoreValue);
        }
    }
}
