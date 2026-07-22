namespace VentionTask1.Domain.Constants
{
    public static class FileUploadConstants
    {
        public const long MaxFileSize = 50 * 1024 * 1024;

        public static readonly Dictionary<string, string[]> AllowedFileTypes =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    [".pdf"] = ["application/pdf"],
                    [".doc"] = ["application/msword"],
                    [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
                    [".txt"] = ["text/plain"],

                    [".png"] = ["image/png"],
                    [".jpg"] = ["image/jpeg"],
                    [".jpeg"] = ["image/jpeg"],
                    [".webp"] = ["image/webp"]
                };
    }
}
