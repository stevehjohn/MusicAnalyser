using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MusicAnalyser.Core;
using NAudio.Wave;
using Newtonsoft.Json;
using static System.Console;

namespace MusicAnalyser.Console
{
    public static class Program
    {
        public static void Main()
        {
            const string path = "C:\\Users\\Stevo\\Music\\Iron Maiden";

            Dictionary<string, ulong[]> fingerprints;

            if (! File.Exists("cache.json"))
            {
                var stopwatch = new Stopwatch();

                stopwatch.Start();

                WriteLine("Cache not found, generating fingerprints.");

                fingerprints = GetFingerprints(path);

                stopwatch.Stop();

                WriteLine($"{fingerprints.Count} fingerprints generated in {(int) stopwatch.Elapsed.TotalSeconds}s");

                File.WriteAllText("cache.json", JsonConvert.SerializeObject(fingerprints));
            }
            else
            {
                WriteLine("Cache found.");

                fingerprints = JsonConvert.DeserializeObject<Dictionary<string, ulong[]>>(File.ReadAllText("cache.json"));
            }

            WriteLine("Press enter to start listening...");

            ReadLine();

            using var source = new WaveInEvent();

            var buffer = new byte[source.WaveFormat.AverageBytesPerSecond * 20];

            var stream = new WaveFileWriter("temp.wav", source.WaveFormat);

            source.DataAvailable += (s, a) =>
            {
                if (stream.Position + a.BytesRecorded > buffer.Length)
                {
                    return;
                }

                stream.Write(a.Buffer, 0, a.BytesRecorded);
            };

            source.StartRecording();

            for (var i = 0; i < 20; i++)
            {
                WriteLine(i);

                Thread.Sleep(1000);
            }

            source.StopRecording();

            stream.Close();
            stream.Dispose();

            var fingerprint = Fingerprinter.GetFingerprint("temp.wav");
        }

        private static Dictionary<string, ulong[]> GetFingerprints(string path)
        {
            var files = Directory.EnumerateFiles(path, "*.mp3", SearchOption.AllDirectories);

            var fingerprints = new Dictionary<string, ulong[]>();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                if (fingerprints.ContainsKey(fileName)) 
                { 
                    continue;
                }

                Write($"Fingerprinting {fileName}... ");

                var stopwatch = new Stopwatch();

                stopwatch.Start();

                var fingerprint = Fingerprinter.GetFingerprint(file);

                stopwatch.Stop();

                WriteLine($"{stopwatch.ElapsedMilliseconds:##0,000}ms");

                fingerprints.Add(Path.GetFileName(file), fingerprint);
            }

            return fingerprints;
        }
    }
}
