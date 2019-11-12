using System;
using System.Collections.Generic;
using System.Diagnostics;
using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace MusicAnalyser.Core
{
    public static class Fingerprinter
    {
        private const int ChunkSize = 1024;

        public static ulong[] GetFingerprint(string filename)
        {
            using var data = new AudioFileReader(filename);

            var monoProvider = new StereoToMonoSampleProvider(data)
                       {
                           LeftVolume = 1.0f,
                           RightVolume = 1.0f
                       };

            var mono = monoProvider.ToMono();

            var buffer = new float[ChunkSize];

            var transformed = new List<Complex[]>();

            var m = (int) Math.Log(ChunkSize, 2.0);

            while (true)
            {
                var read = mono.Read(buffer, 0, ChunkSize);

                if (read == 0)
                {
                    break;
                }

                var complex = new Complex[ChunkSize];

                for (var i = 0; i < read; i++)
                {
                    complex[i].X = buffer[i];
                    complex[i].Y = 0;
                }

                FastFourierTransform.FFT(false, m, complex);

                transformed.Add(complex);
            }

            var keyPoints = new List<ulong>();

            foreach (var item in transformed)
            {
                keyPoints.Add(GetKeyPoints(item));
            }

            return keyPoints.ToArray();
        }

        private static ulong GetKeyPoints(Complex[] data)
        {
            var points = new byte[6];

            var  ranges = new[] { 0, 10, 20, 40, 80, 160, 510 };

            for (var i = 0; i < ranges.Length - 1; i++)
            {
                var max = 0.0d;

                for (var f = ranges[i]; f < ranges[i + 1]; f++)
                {
                    var mag = Math.Sqrt(Math.Pow(Math.Abs(data[f].X), 2) + Math.Pow(Math.Abs(data[f].Y), 2));

                    if (mag > max)
                    {
                        max = mag;
                    }
                }

                points[i] = (byte) max;
            }

            return points[0] + ((ulong) points[1] << 8) + ((ulong) points[2] << 16) + ((ulong) points[3] << 24) + ((ulong) points[4] << 32) + ((ulong) points[5] << 40);
        }
    }
}
