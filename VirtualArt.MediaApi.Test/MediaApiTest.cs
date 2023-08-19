using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using VisualArt.MediaApi.Models;
using VisualArt.MediaApi.Services;

namespace VirtualArt.MediaApi.Test
{
    public class MediaApiTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public MediaApiTest(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5118");
            _factory = factory;
        }

        [Fact]
        public async Task First_UploadTest()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var service = serviceProvider.GetRequiredService<ILocalMediaStorageService>();
            service.ReCreateDirectory();

            var file1 = File.ReadAllBytes(@"OriginalMedia\3840x1080-Wallpaper-131.jpg");
            var file2 = File.ReadAllBytes(@"OriginalMedia\Dual-Monitor-Wallpaper-39.jpg");

            using var content1 = new ByteArrayContent(file1);
            using var content2 = new ByteArrayContent(file2);

            using var formData = new MultipartFormDataContent
            {
                { content1, "files", "3840x1080-Wallpaper-131.jpg" },
                { content2, "files", "Dual-Monitor-Wallpaper-39.jpg" }
            };

            // add two new media
            var response = await _client.PostAsync("/Media", formData);

            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<StandardApiObjectResponse<IEnumerable<UploadMediaFileResponse>>>();

            Assert.Equal(2, result?.Result.Count());
            Assert.True(result?.Result.All(x => x.Saved));

            // going to upload exist media with same filename and different media content:

            using var content3 = new ByteArrayContent(file2);
            using var formData2 = new MultipartFormDataContent
            {
                { content3, "files", "3840x1080-Wallpaper-131.jpg" }
            };

            response = await _client.PostAsync("/Media", formData2);

            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            result = await response.Content.ReadFromJsonAsync<StandardApiObjectResponse<IEnumerable<UploadMediaFileResponse>>>();

            Assert.Equal(1, result?.Result.Count());
            Assert.True(result?.Result.All(x => x.Saved));


            // going to upload exist media with same filename and same media content:

            using var content4 = new ByteArrayContent(file2);
            using var formData3 = new MultipartFormDataContent
            {
                { content4, "files", "3840x1080-Wallpaper-131.jpg" }
            };

            response = await _client.PostAsync("/Media", formData3);

            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            result = await response.Content.ReadFromJsonAsync<StandardApiObjectResponse<IEnumerable<UploadMediaFileResponse>>>();

            Assert.Equal(1, result?.Result.Count());
            Assert.True(result?.Result.All(x => x.Saved is false));
        }

        [Fact]
        public async Task Second_GetSampleMediaFiles()
        {
            // Act
            var response = await _client.GetAsync("/Media");

            response.EnsureSuccessStatusCode();
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = response.Content.ReadFromJsonAsync<StandardApiObjectResponse<IEnumerable<MediaModel>>>();

            Assert.Equal(2, result?.Result?.Result?.Count());
        }
    }
}