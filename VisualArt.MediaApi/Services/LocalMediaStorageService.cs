using VisualArt.MediaApi.Models;

namespace VisualArt.MediaApi.Services
{
    public class LocalMediaStorageService : ILocalMediaStorageService
    {
        private readonly ILogger<LocalMediaStorageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly DirectoryInfo _directoryInfo;

        public LocalMediaStorageService(ILogger<LocalMediaStorageService> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _directoryInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"));
        }

        public IEnumerable<MediaModel> GetPagedFiles(int page, int pageSize)
        {
            var files = _directoryInfo.GetFiles()?.OrderByDescending(x => x.CreationTime)
                .Skip(page - 1 * pageSize)
                .Take(pageSize) ?? Array.Empty<FileInfo>();

            return MapFiles(files);
        }

        public void ReCreateDirectory()
        {
            if (_directoryInfo.Exists is false)
            {
                _directoryInfo.Create();
            }
            else
            {
                // remove files to handle in test project;
                if (_webHostEnvironment.IsProduction() is false)
                    _directoryInfo.Delete(true);

                _directoryInfo.Create();
            }
        }

        public async Task<IEnumerable<UploadMediaFileResponse>> SaveMediaItems(List<IFormFile> files)
        {
            ValidateFiles(files);

            // We will return key value dictionary to detect not uploaded files(false).
            var result = new List<UploadMediaFileResponse>();
            
            foreach (var file in files)
            {
                var path = Path.Combine(_directoryInfo.FullName, file.FileName);
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var newFileBody = memoryStream.ToArray();

                var existFileOnlyByFilename = _directoryInfo.GetFiles(file.FileName).FirstOrDefault();

                if (existFileOnlyByFilename?.Name.Equals(file.FileName, StringComparison.InvariantCultureIgnoreCase) is true)
                {
                    var existFileBody = await File.ReadAllBytesAsync(existFileOnlyByFilename.FullName);

                    // If file exists by filename, compare file content to prevent rewriting the same file with same name.
                    if (existFileBody.SequenceEqual(newFileBody))
                    {
                        //should not store file, because file is exist by file content
                        result.Add(new(file.FileName, false));
                    }
                    else
                    {
                        //file with the same name is exists but content is different with file on server, therefore it will update
                        await File.WriteAllBytesAsync(path, newFileBody);
                        _logger.LogWarning($"file with the same name {file.FileName} is exists but content is different with file on server, therefore it will update and replace");
                        result.Add(new(file.FileName, true));
                    }
                }
                else
                {
                    /*
                     * Note: We can also go on by only focus on File Content, not filename. 
                     * And we can have a unique approach for filename generation and we can check file existence only by file content and return new server file name or ID.
                     * But in this Code Practice, you mentioned "simple Media Web API", so we are going to compare files by filename, then by file content.
                     */
                    await File.WriteAllBytesAsync(path, newFileBody);
                    result.Add(new(file.FileName, true));
                }
            }
            return result;
        }

        private void ValidateFiles(List<IFormFile> files)
        {
            static long GetFileSizeBytes(int maxSizeLimitInMB) => maxSizeLimitInMB * 1024 * 1024;

            var maxSizeLimitInMB = _configuration.GetValue<int>("MaxFileSizeInMB");

            if (files.Exists(x => x.Length > GetFileSizeBytes(maxSizeLimitInMB)))
                throw new CustomException($"At least one file has invalid file size (>{maxSizeLimitInMB}MB)");
        }

        private IEnumerable<MediaModel> MapFiles(IEnumerable<FileInfo> files)
        {
            return files.Select(x => new MediaModel
            {
                CreateDatetime = x.CreationTime,
                FileName = x.Name,
                FileSizeInBytes = x.Length,
                LasModifiedDatetime = x.LastWriteTime
            });
        }
    }
}
