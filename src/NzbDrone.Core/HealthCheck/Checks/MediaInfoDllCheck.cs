using System;
using System.Runtime.CompilerServices;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MediaInfoDllCheck : HealthCheckBase
    {
        public MediaInfoDllCheck(ILocalizationService localizationService)
            : base(localizationService)
        {
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public override HealthCheck Check()
        {
            try
            {
                var mediaInfo = new MediaInfo();
            }
            catch (Exception e)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format(_localizationService.GetLocalizedString("MediaInfoDllCheckMessage"), e.Message), "#mediainfo_not_loaded");
            }

            return new HealthCheck(GetType());
        }
    }
}
