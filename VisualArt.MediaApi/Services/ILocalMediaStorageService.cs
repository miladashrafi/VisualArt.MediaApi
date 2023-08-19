using VisualArt.MediaApi.Models;

namespace VisualArt.MediaApi.Services
{
    public interface ILocalMediaStorageService
    {
        IEnumerable<MediaModel> GetPagedFiles(int page, int pageSize);
        Task<IEnumerable<UploadMediaFileResponse>> SaveMediaItems(List<IFormFile> files);
        void ReCreateDirectory();
    }
}