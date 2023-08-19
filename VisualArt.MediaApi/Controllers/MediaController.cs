using Microsoft.AspNetCore.Mvc;
using VisualArt.MediaApi.Filters;
using VisualArt.MediaApi.Models;
using VisualArt.MediaApi.Services;

namespace VisualArt.MediaApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [TypeFilter(typeof(ApiExceptionHandlerAttribute))]
    public class MediaController : ControllerBase
    {

        private readonly ILocalMediaStorageService _localMediaStorageService;

        public MediaController(ILocalMediaStorageService localMediaStorageService)
        {
            _localMediaStorageService = localMediaStorageService;
        }

        [HttpGet]
        public IEnumerable<MediaModel> GetMediaList([FromQuery] GetMediaListRequestModel request)
            => _localMediaStorageService.GetPagedFiles(request.Page, request.PageSize);

        [HttpPost]
        public async Task<IEnumerable<UploadMediaFileResponse>> UploadMedia(List<IFormFile> files)
        {
            return await _localMediaStorageService.SaveMediaItems(files);
        }
    }
}
