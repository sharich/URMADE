using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System;
using System.Drawing;
using System.IO;
using System.Web;

namespace URMade
{
    public class FileUploadManager
    {
        public static string UploadFile(HttpPostedFileBase file, string uploadType, string fileId)
        {
            if (!ValidUploadExtension(uploadType, file)) return null;

            string extension = Path.GetExtension(file.FileName);
            string title = Path.GetFileNameWithoutExtension(file.FileName);

            string fileSavePath = FilePath(uploadType, fileId, extension);
            string uploadFolder = Path.GetDirectoryName(fileSavePath);

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            if (uploadType == "ProfilePhoto" || uploadType == "AlbumArt" || uploadType == "ArtistImage" || uploadType == "BannerImage")
            {

                ISupportedImageFormat format = new JpegFormat { Quality = 70 };
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                {
                    imageFactory.Load(file.InputStream)
                        .Resize(new ResizeLayer(GetImageSize(uploadType))
                        {
                            ResizeMode = GetImageCrop(uploadType)
                        })
                        .Format(format)
                        .Save(fileSavePath);
                }
            } else {
                file.SaveAs(fileSavePath);
            }

            return "/Content/" + uploadType + "/" + fileId + extension;
        }
        public static Size GetImageSize(string uploadType)
        {
            if (uploadType == "ProfilePhoto" || uploadType == "ArtistImage") return new Size(500, 375);
            if (uploadType == "BannerImage") return new Size(970, 350);
            if (uploadType == "AlbumArt") return new Size(500, 500);
            return new Size(1200, 1200);
        }

        public static ResizeMode GetImageCrop(string uploadType)
        {
            if (uploadType == "ProfilePhoto" || uploadType == "ArtistImage") return ResizeMode.Max;
            if (uploadType == "BannerImage") return ResizeMode.Crop;
            if (uploadType == "AlbumArt") return ResizeMode.Crop;
            return ResizeMode.Crop;
        }

        public static bool ValidUploadExtension(string uploadType, HttpPostedFileBase file)
        {
            if (file == null || String.IsNullOrEmpty(file.FileName)) return false;

            string extension = Path.GetExtension(file.FileName).ToLower();

            if (uploadType == "ProfilePhoto" || uploadType == "AlbumArt" || uploadType == "ArtistImage" || uploadType == "BannerImage") return extension == ".jpg" || extension == ".jpeg" || extension == ".png";

            if (uploadType == "Song") return extension == ".mp3";

            return extension == ".jpg";
        }

        public static void DeleteFileUpload(string path)
        {
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.TrimStart('/'));
            try
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
            }
            catch { }
        }

        public static string FilePath(string uploadType, string fileId, string extension)
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Content",
                 uploadType,
                String.Format("{0}{1}", fileId, extension));
        }
    }
}