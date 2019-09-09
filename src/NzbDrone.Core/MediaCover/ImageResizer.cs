using System;
using NzbDrone.Common.Disk;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Memory;

namespace NzbDrone.Core.MediaCover
{
    public interface IImageResizer
    {
        void Resize(string source, string destination, int height);
    }

    public class ImageResizer : IImageResizer
    {
        private readonly IDiskProvider _diskProvider;

        public ImageResizer(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;

            // More conservative memory allocation
            SixLabors.ImageSharp.Configuration.Default.MemoryAllocator = new SimpleGcMemoryAllocator();
        }

        public void Resize(string source, string destination, int height)
        {
            try
            {
                using (var image = Image.Load(source))
                {
                    var width = (int)Math.Floor((double)image.Width * (double)height / (double)image.Height);
                    image.Mutate(x => x.Resize(width, height));
                    image.Save(destination);
                }
            }
            catch
            {
                if (_diskProvider.FileExists(destination))
                {
                    _diskProvider.DeleteFile(destination);
                }
                throw;
            }
        }
    }
}
