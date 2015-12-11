//$URL: https://ultrastardx.svn.sourceforge.net/svnroot/ultrastardx/trunk/src/base/URecord.pas $

using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityStar.Assets.Song.Audio
{
    public class FrequencyExtractor : MonoBehaviour
    {
        private const float BaseToneFreq = 65.4064f;
        private const int HalfTonesCount = 46;
        private const int SampleCount = 4096;
        private const float HalfToneBase = 1.05946309436f;

        private string[] ToneStrings = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        private float[] _analysisData;
        private float _samplingFreq;

        [Range(0, 1)]
        public float MinVol = 0;

        public bool HasTone { get; private set; } // Is there a tone for now ?
        public int Tone { get; private set; } // Tone ignoring octave 0 - 11
        public int ToneAbs { get; private set; } //Full tone 0 - HalfTonesCount - 1

        private MicControlC _micControl;

        // Use this for initialization
        private void Start()
        {
            _micControl = GetComponent<MicControlC>();

            _analysisData = new float[SampleCount];
            GetComponent<AudioSource>().mute = true;
        }

        // Update is called once per frame
        private void Update()
        {
            if (_samplingFreq == 0)
            {
                _samplingFreq = AudioSettings.outputSampleRate;
                Debug.Log(_samplingFreq);
            }

            var source = GetComponent<AudioSource>();
            source.GetOutputData(_analysisData, 0);

            //Get max vol on first 1024 ech
            var vol = _analysisData.Select(x => Math.Abs(x)).Max();

            if (vol > MinVol)
            {
                AnalyzeByAutocorrelation();
                HasTone = true;
            }
            else
            {
                HasTone = false;
            }

            //        if(HasTone)
            //          Debug.Log(GetToneString());
        }

        private void AnalyzeByAutocorrelation()
        {
            float maxWeight = -1;
            int maxTone = 0;

            for (int i = 0; i < HalfTonesCount; i++)
            {
                float curFreq = BaseToneFreq * Mathf.Pow(HalfToneBase, i);
                float curWeight = AnalyzeByAutocorrelationFreq(curFreq);

                if (curWeight > maxWeight)
                {
                    maxWeight = curWeight;
                    maxTone = i;
                }
            }

            ToneAbs = maxTone;
            Tone = maxTone % 12;
        }

        private float AnalyzeByAutocorrelationFreq(float freq)
        {
            int sampleIndex = 0;
            int samplePerPeriod = (int)Mathf.Round(_samplingFreq / freq);
            int correlatingSampleIndex = sampleIndex + samplePerPeriod;
            float accDist = 0;

            //Unity sample varies from -1 to 1
            while (correlatingSampleIndex < SampleCount)
            {
                float sample = (_analysisData[sampleIndex] + 1) / 2;
                float corrSample = (_analysisData[correlatingSampleIndex] + 1) / 2;

                float dist = Mathf.Abs(sample - corrSample);
                accDist += dist;

                sampleIndex++;
                correlatingSampleIndex++;
            }

            return 1 - (accDist / SampleCount);
        }

        private string GetToneString()
        {
            if (HasTone)
            {
                return ToneStrings[Tone] + (ToneAbs / 12 + 2) + " " + BaseToneFreq * Mathf.Pow(HalfToneBase, Tone);
            }
            else
            {
                return "-";
            }
        }
    }
}