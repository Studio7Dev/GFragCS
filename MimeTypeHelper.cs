using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFrag
{
    public class MimeTypeHelper
    {
        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // Text types
        { ".html", "text/html" },
        { ".htm", "text/html" },
        { ".css", "text/css" },
        { ".js", "text/javascript" },
        { ".txt", "text/plain" },
        { ".xml", "text/xml" },

        // Image types
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".ico", "image/x-icon" },
        { ".svg", "image/svg+xml" },
        { ".webp", "image/webp" },

        // Audio types
        { ".mp3", "audio/mpeg" },
        { ".wav", "audio/wav" },
        { ".ogg", "audio/ogg" },
        { ".aac", "audio/aac" },

        // Video types
        { ".mp4", "video/mp4" },
        { ".avi", "video/x-msvideo" },
        { ".mov", "video/quicktime" },
        { ".wmv", "video/x-ms-wmv" },
        { ".flv", "video/x-flv" },
        { ".mkv", "video/x-matroska" },

        // Application types
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".zip", "application/zip" },
        { ".rar", "application/x-rar-compressed" },
        { ".exe", "application/x-msdownload" },
    };

        public static string GetMimeType(string fileNameOrExtension)
        {
            if (string.IsNullOrWhiteSpace(fileNameOrExtension))
            {
                return null; // or throw an exception, depending on your requirements
            }

            // Extract the extension from the filename (if any)
            string extension = Path.GetExtension(fileNameOrExtension);

            // If no extension was found, treat the input as an extension (e.g., "html")
            if (string.IsNullOrEmpty(extension))
            {
                extension = "." + fileNameOrExtension; // Ensure it starts with a dot for lookup
            }

            return MimeTypes.TryGetValue(extension, out string mimeType) ? mimeType : "application/octet-stream";
        }
    }
}
