
using System.Diagnostics;


public class FileSplitter
{
    public static void ReassembleFile(string outputFilePath, string inputDir, string originalFilename)
    {
        using (var outputFile = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
        {
            var partNumber = 1;
            while (true)
            {
                var fragmentFilePath = Path.Combine(inputDir, $"{originalFilename}_part_{partNumber}.part");

                if (!File.Exists(fragmentFilePath))
                {
                    break;
                }

                using (var fragmentFile = new FileStream(fragmentFilePath, FileMode.Open, FileAccess.Read))
                {
                    fragmentFile.CopyTo(outputFile);
                    Debug.WriteLine("Copying Buffer to "+ outputFilePath);
                }

                partNumber++;
                Debug.WriteLine(fragmentFilePath);
            }
        }

        // Delete the input directory
        try
        {
            Directory.Delete(inputDir, true);
            Debug.WriteLine("Input directory deleted successfully.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to delete input directory: " + ex.Message);
        }
    }

    public static string FragmentFile(string sourceFilePath, string outputDir)
    {
        using (var file = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
        {
            var fileInfo = new FileInfo(sourceFilePath);
            double fileSplitPercent = Preferences.Get("FGP", 0.05);
            // Calculate fragment size as 5% of the total file size
            var fragmentSize = (long)(fileInfo.Length * fileSplitPercent);

            // If the file is smaller than the calculated fragment size, use the file size as the fragment size
            if (fragmentSize > fileInfo.Length)
            {
                fragmentSize = fileInfo.Length;
            }

            var filename = Path.GetFileName(sourceFilePath);
            var partNumber = 1;
            var written = 0L;

            file.Position = 0; // Reset file pointer to the beginning for reading

            while (written < fileInfo.Length)
            {
                var fragmentFilePath = Path.Combine(outputDir, $"{filename}_part_{partNumber}.part");
                using (var fragmentFile = new FileStream(fragmentFilePath, FileMode.Create, FileAccess.Write))
                {
                    var bytesToWrite = (int)Math.Min(fragmentSize, fileInfo.Length - written);

                    var buffer = new byte[bytesToWrite];
                    file.Read(buffer, 0, bytesToWrite);
                    fragmentFile.Write(buffer, 0, bytesToWrite);

                    written += bytesToWrite;
                    partNumber++;
                }
            }
        }

        return outputDir;
    }
}