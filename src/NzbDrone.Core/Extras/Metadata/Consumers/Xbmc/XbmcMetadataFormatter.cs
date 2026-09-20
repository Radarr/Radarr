using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public static class XbmcMetadataFormatter
    {
        public static string FormatAudioCodec(MediaInfoModel mediaInfo)
        {
            if (mediaInfo == null)
            {
                return string.Empty;
            }

            var audioFormat = mediaInfo.AudioFormat;
            var audioCodecID = mediaInfo.AudioCodecID ?? string.Empty;
            var audioProfile = mediaInfo.AudioProfile ?? string.Empty;

            // profile name definitions here https://github.com/FFmpeg/FFmpeg/blob/n5.1.4/libavcodec/profiles.c
            // ffmpeg 5.1.4 doesn't support the profiles "Dolby Digital Plus + Dolby Atmos" and "Dolby TrueHD + Dolby Atmos"
            // A custom ffmpeg patch maps them as codec_tag_string values "ec+3" and "thd+" respectively.
            return audioCodecID switch
            {
                "thd+" => "truehd_atmos",
                "ec+3" => "eac3_ddp_atmos",
                _ => audioFormat switch
                {
                    // Missing Kodi dedicated codes for "DTS-ES" "DTS Express" "DTS 96/24"
                    // ffmpeg 5.1.4 doesn't support "DTS-HD MA + DTS:X" and "DTS-HD MA + DTS:X IMAX" for dtshd_ma_x and dtshd_ma_x_imax
                    // A custom ffmpeg patch identifies both as profile "DTS:X"
                    "dts" => audioProfile switch
                    {
                        "DTS:X" => "dtshd_ma_x",
                        "DTS-HD HRA" => "dtshd_hra",
                        "DTS-HD MA" => "dtshd_ma",
                        _ => audioFormat,
                    },

                    // Missing Kodi dedicated codes for "LD", "ELD", "Main", "xHE-AAC"
                    "aac" => audioProfile switch
                    {
                        "LC" => "aac_lc",
                        "HE-AAC" => "he_aac",
                        "HE-AACv2" => "he_aac_v2",
                        "SSR" => "aac_ssr",
                        "LTP" => "aac_ltp",
                        _ => audioFormat,
                    },
                    _ => audioFormat
                }
            };
        }
    }
}
