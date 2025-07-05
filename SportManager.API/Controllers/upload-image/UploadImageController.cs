using Microsoft.AspNetCore.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SportManager.API.Controllers.upload_image;

[Route("api/upload-image")]
[ApiController]
public class UploadImageController : ApiControllerBase
{
    // private readonly IWebHostEnvironment _env;

    private readonly Cloudinary _cloudinary; // Thêm Cloudinary instance
    private readonly ILogger<UploadImageController> _logger;

    public UploadImageController(Cloudinary cloudinary, ILogger<UploadImageController> logger)
    {
        _cloudinary = cloudinary;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> UploadImagesAsync(List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded");

        var urls = new List<string>();
        var publicIds = new List<string>(); //lưu PublicId của Cloudinary để có thể xóa sau này

        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                _logger.LogWarning("Skipping empty file: {FileName}", file.FileName);
                continue;
            }

            try
            {
                using var stream = file.OpenReadStream();

                // Tạo PublicId duy nhất cho ảnh trên Cloudinary
                // Bạn có thể thêm folder vào PublicId nếu muốn tổ chức file trên Cloudinary
                var uniquePublicId = $"sport-manager/{Guid.NewGuid()}"; // Ví dụ: folder/guid

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = uniquePublicId, // Thiết lập PublicId
                    Overwrite = true, // Tùy chọn: ghi đè nếu PublicId đã tồn tại (ít khi xảy ra với Guid)
                    // Folder = "sport-manager-images", // Tùy chọn: Tổ chức trong một thư mục trên Cloudinary Dashboard
                    UseFilename = false, // Không dùng tên file gốc, dùng PublicId
                    UniqueFilename = false // PublicId đã unique rồi
                };

                // Thực hiện upload lên Cloudinary
                var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    urls.Add(uploadResult.SecureUrl.ToString()); // Lấy HTTPS URL của ảnh đã upload
                    publicIds.Add(uniquePublicId); // Lưu lại PublicId để xóa sau này
                    _logger.LogInformation("Uploaded {FileName} to Cloudinary: {Url}", file.FileName, uploadResult.SecureUrl);
                }
                else
                {
                    _logger.LogError("Cloudinary upload failed for {FileName}: {Error}", file.FileName, uploadResult.Error?.Message);
                    return StatusCode(500, $"Failed to upload file {file.FileName}: {uploadResult.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName} to Cloudinary", file.FileName);
                return StatusCode(500, $"Internal server error while uploading {file.FileName}: {ex.Message}");
            }
        }

        // Trả về danh sách URL của ảnh và PublicId nếu cần
        // Hiện tại chỉ trả về URLs, nhưng bạn cần lưu PublicId vào DB cùng với URL để có thể xóa.
        return Ok(urls);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteImage([FromQuery] string publicId) // Tham số giờ là publicId
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return BadRequest("Public ID is required");

        try
        {
            var deletionParams = new DeletionParams(publicId);

            var deleteResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deleteResult.Result == "ok" || deleteResult.Result == "not found")
            {
                _logger.LogInformation("Deleted image with Public ID: {PublicId}. Result: {Result}", publicId, deleteResult.Result);
                return Ok("File deleted successfully");
            }
            else
            {
                _logger.LogError("Cloudinary deletion failed for Public ID {PublicId}: {Error}", publicId, deleteResult.Error?.Message);
                return StatusCode(500, $"Failed to delete file {publicId}: {deleteResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image with Public ID {PublicId} from Cloudinary", publicId);
            return StatusCode(500, $"Internal server error while deleting {publicId}: {ex.Message}");
        }
    }

    // =============================================================
    // COMMENT CODE CŨ (LOCAL UPLOAD)
    // =============================================================

    /*
    // Cũ: Controller khởi tạo với IWebHostEnvironment
    // public UploadImageController(IWebHostEnvironment env)
    // {
    //     _env = env;
    // }

    // Cũ: Hàm UploadImagesAsync lưu cục bộ
    // [HttpPost]
    // public async Task<IActionResult> UploadImagesAsync(List<IFormFile> files, CancellationToken cancellationToken)
    // {
    //     if (files == null || files.Count == 0)
    //         return BadRequest("No files uploaded");

    //     var urls = new List<string>();

    //     foreach (var file in files)
    //     {
    //         var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    //         var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

    //         Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

    //         using var stream = new FileStream(filePath, FileMode.Create);
    //         await file.CopyToAsync(stream, cancellationToken);

    //         urls.Add($"/uploads/{fileName}");
    //     }

    //     return Ok(urls);
    // }

    // Cũ: Hàm DeleteImage xóa cục bộ
    // [HttpDelete]
    // public IActionResult DeleteImage([FromQuery] string fileName)
    // {
    //     if (string.IsNullOrWhiteSpace(fileName))
    //         return BadRequest("File name is required");

    //     var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

    //     if (!System.IO.File.Exists(filePath))
    //         return NotFound("File not found");

    //     System.IO.File.Delete(filePath);

    //     return Ok("File deleted successfully");
    // }
    */
}