using System;

namespace TGBox.Models
{
    public class VideoFile
    {
        public string FilePath { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
    }
}