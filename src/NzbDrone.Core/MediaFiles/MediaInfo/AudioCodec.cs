namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public enum AudioCodec
    {
        Unknown = 0,
        MP2 = 1,
        MP3 = 2,
        PCM = 3,
        Vorbis = 4,
        WMA = 5,
        Opus = 6,
        AAC = 7,
        HE_AAC = 8,
        AC3 = 9,
        FLAC = 10,
        EAC3 = 11,
        DTS = 12,
        DTS_Express = 13,
        DTS_9624 = 14,
        DTS_ES = 15,
        DTS_HD_HRA = 16,
        EAC3Atmos = 17,
        DTS_HD_MA = 18,
        DTS_X = 19,
        TrueHD = 20,
        TrueHDAtmos = 21
    }

    public static class AudioCodecHelper
    {
        public static AudioCodec Resolve(string format, string codecID, string profile)
        {
            format = format ?? string.Empty;
            codecID = codecID ?? string.Empty;
            profile = profile ?? string.Empty;

            if (codecID == "thd+")
            {
                return AudioCodec.TrueHDAtmos;
            }

            if (format == "truehd")
            {
                return AudioCodec.TrueHD;
            }

            if (format == "flac")
            {
                return AudioCodec.FLAC;
            }

            if (format == "dts")
            {
                if (profile == "DTS:X")
                {
                    return AudioCodec.DTS_X;
                }

                if (profile == "DTS-HD MA")
                {
                    return AudioCodec.DTS_HD_MA;
                }

                if (profile == "DTS-ES")
                {
                    return AudioCodec.DTS_ES;
                }

                if (profile == "DTS-HD HRA")
                {
                    return AudioCodec.DTS_HD_HRA;
                }

                if (profile == "DTS Express")
                {
                    return AudioCodec.DTS_Express;
                }

                if (profile == "DTS 96/24")
                {
                    return AudioCodec.DTS_9624;
                }

                return AudioCodec.DTS;
            }

            if (codecID == "ec+3")
            {
                return AudioCodec.EAC3Atmos;
            }

            if (format == "eac3")
            {
                return AudioCodec.EAC3;
            }

            if (format == "ac3")
            {
                return AudioCodec.AC3;
            }

            if (format == "aac")
            {
                if (codecID == "A_AAC/MPEG4/LC/SBR")
                {
                    return AudioCodec.HE_AAC;
                }

                return AudioCodec.AAC;
            }

            if (format == "mp3")
            {
                return AudioCodec.MP3;
            }

            if (format == "mp2")
            {
                return AudioCodec.MP2;
            }

            if (format == "opus")
            {
                return AudioCodec.Opus;
            }

            if (format.StartsWith("pcm_") || format.StartsWith("adpcm_"))
            {
                return AudioCodec.PCM;
            }

            if (format == "vorbis")
            {
                return AudioCodec.Vorbis;
            }

            if (format == "wmav1" ||
                format == "wmav2" ||
                format == "wmapro")
            {
                return AudioCodec.WMA;
            }

            return AudioCodec.Unknown;
        }

        public static string GetDisplayName(AudioCodec codec)
        {
            return codec switch
            {
                AudioCodec.TrueHDAtmos => "TrueHD Atmos",
                AudioCodec.TrueHD => "TrueHD",
                AudioCodec.DTS_X => "DTS-X",
                AudioCodec.DTS_HD_MA => "DTS-HD MA",
                AudioCodec.DTS_HD_HRA => "DTS-HD HRA",
                AudioCodec.DTS_ES => "DTS-ES",
                AudioCodec.DTS_Express => "DTS Express",
                AudioCodec.DTS_9624 => "DTS 96/24",
                AudioCodec.DTS => "DTS",
                AudioCodec.EAC3Atmos => "EAC3 Atmos",
                AudioCodec.EAC3 => "EAC3",
                AudioCodec.FLAC => "FLAC",
                AudioCodec.AC3 => "AC3",
                AudioCodec.HE_AAC => "HE-AAC",
                AudioCodec.AAC => "AAC",
                AudioCodec.MP3 => "MP3",
                AudioCodec.MP2 => "MP2",
                AudioCodec.Opus => "Opus",
                AudioCodec.PCM => "PCM",
                AudioCodec.Vorbis => "Vorbis",
                AudioCodec.WMA => "WMA",
                _ => string.Empty
            };
        }
    }
}
