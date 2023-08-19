namespace VisualArt.MediaApi.Models
{
    public record class StandardApiResponse(bool Success, string ErrorMessage = null);
    public record class StandardApiObjectResponse<TResult>(bool Success, TResult Result, string ErrorMessage = null) : StandardApiResponse(Success, ErrorMessage);
}
