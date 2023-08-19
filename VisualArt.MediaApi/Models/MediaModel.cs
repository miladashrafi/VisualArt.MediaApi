namespace VisualArt.MediaApi.Models
{
    public class MediaModel
    {
        public string FileName { get; set; }
        public string FileExtension => Path.GetExtension(FileName);
        public long FileSizeInBytes { get; set; }
        public DateTime CreateDatetime { get; set; }
        public DateTime LasModifiedDatetime { get; set; }
    }
}
