using MVCProject.Utilities.Enums;

namespace MVCProject.Utilities
{
    public static class FileValidator
    {
        public static bool IsValidSize(this IFormFile file, FileSize fileSize, int size)
        {
            switch (fileSize)
            {
                case FileSize.KB:
                    return file.Length < size * 1024;
                case FileSize.MB:
                    return file.Length < size * 1024 * 1024;
                case FileSize.GB:
                    return file.Length < size * 1024 * 1024 * 1024;

            }

            return false;
        }

        public static bool IsValidType(this IFormFile file, string type)
        {
            return file.ContentType.Contains(type);
        }

        public static async Task<string> CreateFileAsync(this IFormFile file, params string[] roots)
        {

            string fileName = string.Concat(Guid.NewGuid().ToString(), Path.GetExtension(file.FileName));

            string path = CombinePath(fileName, roots);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public static void DeleteFile(this string fileName, params string[] roots)
        {
            string path = CombinePath(fileName, roots);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

        }

        public static string CombinePath(string fileName, params string[] roots)
        {
            string path = string.Empty;
            foreach (string folder in roots)
            {
                path = Path.Combine(path, folder);
            }
            path = Path.Combine(path, fileName);
            return path;
        }




    }
}

