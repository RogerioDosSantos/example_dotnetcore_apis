
namespace DotNetCoreApis.Models
{
    public class FileUploadResponseModel
    {
        /// <summary>
        /// Name of the uploaded file
        /// </summary>
        public string? file { get; set; }

        /// <summary>
        /// Path where the file was stored in the server
        /// </summary>
        public string? uploadPath { get; set; }

        /// <summary>
        /// Define if the file was replaced
        /// </summary>
        public bool? fileReplaced { get; set; }

        /// <summary>
        /// Uploaded File Size in MB
        /// </summary>
        public float? fileSize { get; set; }

        /// <summary>
        /// Inform the error if any
        /// </summary>
        public string? error { get; set; }
    }
}
