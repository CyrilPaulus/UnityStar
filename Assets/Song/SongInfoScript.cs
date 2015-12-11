using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStar.Assets.Song
{
    public class SongInfoScript : MonoBehaviour
    {
        public Text ArtistText;
        public Text TitleText;
        public Text YearText;
        public Text CountText;

        private int _songsCount = 0;

        // Use this for initialization
        private void Start()
        {
        }

        public void SetCount(int count)
        {
            _songsCount = count;
        }

        public void SetSong(SongData song, int index)
        {
            ArtistText.text = song.Artist;
            TitleText.text = song.Title;
            if (!string.IsNullOrEmpty(song.Year))
                YearText.text = string.Format("({0})", song.Year);
            else
                YearText.text = string.Empty;
            CountText.text = string.Format("{0}/{1}", index, _songsCount);
        }
    }
}