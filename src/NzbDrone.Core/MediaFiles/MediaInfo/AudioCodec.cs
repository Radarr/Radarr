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
            format ??= string.Empty;
            codecID ??= string.Empty;
            profile ??= string.Empty;

            return (codecID, format, profile) switch
            {
                ("thd+", _, _) => AudioCodec.TrueHDAtmos,
                ("ec+3", _, _) => AudioCodec.EAC3Atmos,
                (_, "truehd", _) => AudioCodec.TrueHD,
                (_, "flac", _) => AudioCodec.FLAC,
                (_, "dts", "DTS:X") => AudioCodec.DTS_X,
                (_, "dts", "DTS-HD MA") => AudioCodec.DTS_HD_MA,
                (_, "dts", "DTS-HD HRA") => AudioCodec.DTS_HD_HRA,
                (_, "dts", "DTS-ES") => AudioCodec.DTS_ES,
                (_, "dts", "DTS Express") => AudioCodec.DTS_Express,
                (_, "dts", "DTS 96/24") => AudioCodec.DTS_9624,
                (_, "dts", _) => AudioCodec.DTS,
                (_, "eac3", _) => AudioCodec.EAC3,
                (_, "ac3", _) => AudioCodec.AC3,
                ("A_AAC/MPEG4/LC/SBR", "aac", _) => AudioCodec.HE_AAC,
                (_, "aac", _) => AudioCodec.AAC,
                (_, "mp3", _) => AudioCodec.MP3,
                (_, "mp2", _) => AudioCodec.MP2,
                (_, "opus", _) => AudioCodec.Opus,
                (_, "vorbis", _) => AudioCodec.Vorbis,
                (_, "wmav1", _) => AudioCodec.WMA,
                (_, "wmav2", _) => AudioCodec.WMA,
                (_, "wmapro", _) => AudioCodec.WMA,
                _ when format.StartsWith("pcm_") || format.StartsWith("adpcm_") => AudioCodec.PCM,
                _ => AudioCodec.Unknown
            };
        }

        public static string GetDisplayName(AudioCodec codec) => codec switch
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
