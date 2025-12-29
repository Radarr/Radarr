using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMusicFileAnalyzer
    {
        MusicFileInfo Analyze(string filePath);
        Quality DetermineQuality(string filePath);
    }

    public class MusicFileAnalyzer : IMusicFileAnalyzer
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(MusicFileAnalyzer));

        public MusicFileInfo Analyze(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Logger.Warn("File not found: {0}", filePath);
                return null;
            }

            var info = new MusicFileInfo
            {
                FilePath = filePath,
                Extension = Path.GetExtension(filePath)?.ToLowerInvariant()
            };

            try
            {
                var ffprobeOutput = RunFfprobe(filePath);
                if (ffprobeOutput == null)
                {
                    return info;
                }

                ParseFfprobeOutput(ffprobeOutput, info);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Error analyzing file: {0}", filePath);
            }

            return info;
        }

        public Quality DetermineQuality(string filePath)
        {
            var info = Analyze(filePath);
            if (info == null)
            {
                return Quality.MusicUnknown;
            }

            return DetermineQualityFromInfo(info);
        }

        private static string RunFfprobe(string filePath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        Logger.Debug("Failed to start ffprobe process");
                        return null;
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(30000);

                    if (process.ExitCode != 0)
                    {
                        Logger.Debug("ffprobe exited with code {0}", process.ExitCode);
                        return null;
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Failed to run ffprobe");
                return null;
            }
        }

        private static void ParseFfprobeOutput(string json, MusicFileInfo info)
        {
            var data = JObject.Parse(json);

            var format = data["format"];
            if (format != null)
            {
                info.FormatName = format["format_name"]?.ToString();
                info.Duration = ParseDouble(format["duration"]?.ToString());
                info.Bitrate = ParseInt(format["bit_rate"]?.ToString());

                var tags = format["tags"];
                if (tags != null)
                {
                    info.Title = tags["title"]?.ToString() ?? tags["TITLE"]?.ToString();
                    info.Artist = tags["artist"]?.ToString() ?? tags["ARTIST"]?.ToString();
                    info.Album = tags["album"]?.ToString() ?? tags["ALBUM"]?.ToString();
                }
            }

            var streams = data["streams"] as JArray;
            if (streams == null)
            {
                return;
            }

            foreach (var stream in streams)
            {
                if (stream["codec_type"]?.ToString() != "audio")
                {
                    continue;
                }

                info.Codec = stream["codec_name"]?.ToString();
                info.SampleRate = ParseInt(stream["sample_rate"]?.ToString());
                info.Channels = ParseInt(stream["channels"]?.ToString());

                var bitsPerRawSample = stream["bits_per_raw_sample"]?.ToString();
                var bitsPerSample = stream["bits_per_sample"]?.ToString();
                info.BitDepth = ParseInt(bitsPerRawSample) ?? ParseInt(bitsPerSample);

                if (info.Bitrate == null || info.Bitrate == 0)
                {
                    info.Bitrate = ParseInt(stream["bit_rate"]?.ToString());
                }

                var profile = stream["profile"]?.ToString();
                if (!string.IsNullOrEmpty(profile))
                {
                    info.Profile = profile;
                }

                break;
            }
        }

        private static Quality DetermineQualityFromInfo(MusicFileInfo info)
        {
            var codec = info.Codec?.ToLowerInvariant() ?? string.Empty;
            var ext = info.Extension?.ToLowerInvariant() ?? string.Empty;
            var bitDepth = info.BitDepth ?? 16;
            var sampleRate = info.SampleRate ?? 44100;
            var bitrate = info.Bitrate ?? 0;

            if (IsDsdFormat(codec, ext))
            {
                return DetermineDsdQuality(sampleRate);
            }

            if (IsFlacFormat(codec, ext))
            {
                return DetermineFlacQuality(bitDepth, sampleRate);
            }

            if (IsWavFormat(codec, ext))
            {
                return DetermineWavQuality(bitDepth, sampleRate);
            }

            if (IsAiffFormat(codec, ext))
            {
                return DetermineAiffQuality(bitDepth, sampleRate);
            }

            if (IsAlacFormat(codec, ext))
            {
                return DetermineAlacQuality(bitDepth, sampleRate);
            }

            if (IsApeFormat(codec, ext))
            {
                return Quality.MusicAPE;
            }

            if (IsWavPackFormat(codec, ext))
            {
                return Quality.MusicWavPack;
            }

            if (IsMqaFormat(info))
            {
                return info.Profile?.Contains("Studio") == true
                    ? Quality.MusicMQA_Studio
                    : Quality.MusicMQA;
            }

            if (IsMp3Format(codec, ext))
            {
                return DetermineMp3Quality(bitrate);
            }

            if (IsAacFormat(codec, ext))
            {
                return DetermineAacQuality(bitrate);
            }

            if (IsOggFormat(codec, ext))
            {
                return DetermineOggQuality(bitrate);
            }

            if (IsOpusFormat(codec, ext))
            {
                return DetermineOpusQuality(bitrate);
            }

            if (IsWmaFormat(codec, ext))
            {
                return Quality.MusicWMA;
            }

            return Quality.MusicUnknown;
        }

        private static bool IsDsdFormat(string codec, string ext) =>
            codec.Contains("dsd") || ext == ".dsf" || ext == ".dff";

        private static bool IsFlacFormat(string codec, string ext) =>
            codec == "flac" || ext == ".flac";

        private static bool IsWavFormat(string codec, string ext) =>
            codec == "pcm_s16le" || codec == "pcm_s24le" || codec == "pcm_s32le" ||
            codec.StartsWith("pcm_") || ext == ".wav";

        private static bool IsAiffFormat(string codec, string ext) =>
            codec == "pcm_s16be" || codec == "pcm_s24be" || ext == ".aiff" || ext == ".aif";

        private static bool IsAlacFormat(string codec, string ext) =>
            codec == "alac" || ext == ".m4a";

        private static bool IsApeFormat(string codec, string ext) =>
            codec == "ape" || ext == ".ape";

        private static bool IsWavPackFormat(string codec, string ext) =>
            codec == "wavpack" || ext == ".wv";

        private static bool IsMqaFormat(MusicFileInfo info) =>
            info.FormatName?.Contains("mqa") == true ||
            info.Profile?.Contains("MQA") == true;

        private static bool IsMp3Format(string codec, string ext) =>
            codec == "mp3" || ext == ".mp3";

        private static bool IsAacFormat(string codec, string ext) =>
            codec == "aac" || ext == ".m4a" || ext == ".aac";

        private static bool IsOggFormat(string codec, string ext) =>
            codec == "vorbis" || ext == ".ogg" || ext == ".oga";

        private static bool IsOpusFormat(string codec, string ext) =>
            codec == "opus" || ext == ".opus";

        private static bool IsWmaFormat(string codec, string ext) =>
            codec.Contains("wma") || ext == ".wma";

        private static Quality DetermineDsdQuality(int sampleRate)
        {
            if (sampleRate >= 22000000)
            {
                return Quality.MusicDSD512;
            }

            if (sampleRate >= 11000000)
            {
                return Quality.MusicDSD256;
            }

            if (sampleRate >= 5600000)
            {
                return Quality.MusicDSD128;
            }

            return Quality.MusicDSD64;
        }

        private static Quality DetermineFlacQuality(int bitDepth, int sampleRate)
        {
            if (bitDepth >= 24)
            {
                if (sampleRate >= 176400)
                {
                    return sampleRate >= 192000 ? Quality.MusicFLAC_24_192 : Quality.MusicFLAC_24_176;
                }

                if (sampleRate >= 88200)
                {
                    return sampleRate >= 96000 ? Quality.MusicFLAC_24_96 : Quality.MusicFLAC_24_88;
                }

                return sampleRate >= 48000 ? Quality.MusicFLAC_24_48 : Quality.MusicFLAC_24_44;
            }

            return sampleRate >= 48000 ? Quality.MusicFLAC_16_48 : Quality.MusicFLAC_16_44;
        }

        private static Quality DetermineWavQuality(int bitDepth, int sampleRate)
        {
            if (bitDepth >= 24)
            {
                if (sampleRate >= 176400)
                {
                    return sampleRate >= 192000 ? Quality.MusicWAV_24_192 : Quality.MusicWAV_24_176;
                }

                if (sampleRate >= 88200)
                {
                    return sampleRate >= 96000 ? Quality.MusicWAV_24_96 : Quality.MusicWAV_24_88;
                }

                return sampleRate >= 48000 ? Quality.MusicWAV_24_48 : Quality.MusicWAV_24_44;
            }

            return sampleRate >= 48000 ? Quality.MusicWAV_16_48 : Quality.MusicWAV_16_44;
        }

        private static Quality DetermineAiffQuality(int bitDepth, int sampleRate)
        {
            if (bitDepth >= 24)
            {
                if (sampleRate >= 176400)
                {
                    return sampleRate >= 192000 ? Quality.MusicAIFF_24_192 : Quality.MusicAIFF_24_176;
                }

                if (sampleRate >= 88200)
                {
                    return sampleRate >= 96000 ? Quality.MusicAIFF_24_96 : Quality.MusicAIFF_24_88;
                }

                return sampleRate >= 48000 ? Quality.MusicAIFF_24_48 : Quality.MusicAIFF_24_44;
            }

            return sampleRate >= 48000 ? Quality.MusicAIFF_16_48 : Quality.MusicAIFF_16_44;
        }

        private static Quality DetermineAlacQuality(int bitDepth, int sampleRate)
        {
            if (bitDepth >= 24)
            {
                if (sampleRate >= 176400)
                {
                    return Quality.MusicALAC_24_192;
                }

                if (sampleRate >= 88200)
                {
                    return Quality.MusicALAC_24_96;
                }

                return sampleRate >= 48000 ? Quality.MusicALAC_24_48 : Quality.MusicALAC_24_44;
            }

            return sampleRate >= 48000 ? Quality.MusicALAC_16_48 : Quality.MusicALAC_16_44;
        }

        private static Quality DetermineMp3Quality(int bitrate)
        {
            var kbps = bitrate / 1000;

            if (kbps >= 300)
            {
                return Quality.MusicMP3_320;
            }

            if (kbps >= 240)
            {
                return Quality.MusicMP3_256;
            }

            if (kbps >= 180)
            {
                return Quality.MusicMP3_192;
            }

            return Quality.MusicMP3_128;
        }

        private static Quality DetermineAacQuality(int bitrate)
        {
            var kbps = bitrate / 1000;

            if (kbps >= 300)
            {
                return Quality.MusicAAC_320;
            }

            if (kbps >= 200)
            {
                return Quality.MusicAAC_256;
            }

            return Quality.MusicAAC_128;
        }

        private static Quality DetermineOggQuality(int bitrate)
        {
            var kbps = bitrate / 1000;

            if (kbps >= 300)
            {
                return Quality.MusicOGG_320;
            }

            if (kbps >= 240)
            {
                return Quality.MusicOGG_256;
            }

            if (kbps >= 180)
            {
                return Quality.MusicOGG_192;
            }

            return Quality.MusicOGG_128;
        }

        private static Quality DetermineOpusQuality(int bitrate)
        {
            var kbps = bitrate / 1000;

            if (kbps >= 200)
            {
                return Quality.MusicOpus_256;
            }

            if (kbps >= 160)
            {
                return Quality.MusicOpus_192;
            }

            return Quality.MusicOpus_128;
        }

        private static int? ParseInt(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return int.TryParse(value, out var result) ? result : (int?)null;
        }

        private static double? ParseDouble(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return double.TryParse(value, out var result) ? result : (double?)null;
        }
    }

    public class MusicFileInfo
    {
        public string FilePath { get; set; }
        public string Extension { get; set; }
        public string Codec { get; set; }
        public string FormatName { get; set; }
        public string Profile { get; set; }
        public int? SampleRate { get; set; }
        public int? BitDepth { get; set; }
        public int? Bitrate { get; set; }
        public int? Channels { get; set; }
        public double? Duration { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
    }
}
