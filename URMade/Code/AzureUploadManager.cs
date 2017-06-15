using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Drawing;
using System.Configuration;
using System.Collections.Generic;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace URMade
{
    public static class AzureUploadManager
    {
        private static StorageCredentials storageCredentials = new StorageCredentials(ConfigurationManager.AppSettings["StorageAccountName"], ConfigurationManager.AppSettings["StorageAccountKey"]);
        private static MediaServicesCredentials mediaCredentials = new MediaServicesCredentials(ConfigurationManager.AppSettings["MediaServicesAccountName"], ConfigurationManager.AppSettings["MediaServicesAccountKey"]);
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureConnectionString"]);
        private static CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        private static CloudMediaContext mediaContext = new CloudMediaContext(mediaCredentials);

        public static string UploadFile(HttpPostedFileBase file, string uploadType)
        {
            if (!ValidUploadExtension(uploadType, file))
                return null;

            string fileId = Guid.NewGuid().ToString();

            CloudBlobContainer blobContainer = blobClient.GetContainerReference(uploadType.ToLower());
            blobContainer.CreateIfNotExists();

            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(fileId);

            switch (uploadType)
            {
                case "ProfilePhoto":
                case "AlbumArt":
                case "ArtistImage":
                case "BannerImage":
                case "ContestImage":
                    using (Stream stream = ProcessImage(file.InputStream, uploadType)) {
                        blob.UploadFromStream(stream);
                    }
                    return blob.Uri.AbsoluteUri;
                case "Audio":
                case "Video":
                    blob.UploadFromStream(file.InputStream);
                    return blob.Uri.AbsoluteUri;
            }

            return null;
        }

        public static void DeleteBlob(string uri)
        {
            var BlobRef = GetContainerReferenceFromURI(uri);
            DeleteBlob(BlobRef.Item1, BlobRef.Item2);
        }
        public static void DeleteBlob(string container, string reference)
        {
            try
            {
                CloudBlobContainer Container = blobClient.GetContainerReference(container);
                CloudBlob Blob = Container.GetBlobReference(reference);

                Blob.DeleteIfExists();
            }
            catch { }
        }

        //public static string UploadAndProcessMedia(HttpPostedFileBase file, string uploadType, out string url)
        //{
        //    //if (!ValidUploadExtension(uploadType, file))
        //      //  return null;

        //    string fileId = Guid.NewGuid().ToString();

        //    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "temp", fileId + Path.GetExtension(file.FileName));
        //    file.SaveAs(filePath);
        //    IAsset asset = mediaContext.Assets.CreateFromFile(filePath, AssetCreationOptions.None);
        //    File.Delete(filePath);
        //    string assetId = asset.Id;

        //    //IAsset asset = mediaContext.Assets.Where(p => p.Id == assetId).ToList().FirstOrDefault();
        //    IJob job = mediaContext.Jobs.CreateWithSingleTask("Media Encoder Standard", "Adaptive Streaming", asset, "Adaptive Bitrate MP4", AssetCreationOptions.None);

        //    job.Submit();
        //    job = job.StartExecutionProgressTask(j =>
        //    {
        //        Console.WriteLine("Encode progress: {0}", j.GetOverallProgress());

        //    }, CancellationToken.None).Result;

        //    IAsset encodedAsset = job.OutputMediaAssets[0];
        //    mediaContext.Locators.Create(LocatorType.OnDemandOrigin, encodedAsset, AccessPermissions.Read, TimeSpan.FromDays(30));
        //    mediaContext.Locators.Create(LocatorType.Sas, encodedAsset, AccessPermissions.Read, TimeSpan.FromDays(30));

        //    url = encodedAsset.GetSmoothStreamingUri().AbsoluteUri;
        //    return encodedAsset.Id;
        //}

        public static MediaAsset UploadMedia(HttpPostedFileBase file, string uploadType)
        {
            if (!ValidUploadExtension(uploadType, file))
                return null;

            MediaAsset AssetDetail = new MediaAsset() { FileId = Guid.NewGuid().ToString() };

            // Save temporary file
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", AssetDetail.FileId + Path.GetExtension(file.FileName));
            file.SaveAs(filePath);

            // Upload file to Azure storage and Create Asset
            IAsset asset = mediaContext.Assets.CreateFromFile(filePath, AssetCreationOptions.None);
            AssetDetail.AssetId = asset.Id;
            File.Delete(filePath); // Remove temporary file

            // Create and submit Re-encoding job
            IJob job = mediaContext.Jobs.CreateWithSingleTask("Media Encoder Standard", "Adaptive Streaming", asset, "Adaptive Bitrate MP4", AssetCreationOptions.None);
            job.Submit();
            AssetDetail.JobId =  job.Id;

            return AssetDetail;
          
        }

        /// <summary>
        /// Updates Model from Azure if the SmoothStreamingUri is NullorEmpty. JobId is required. State, EncodedAssetId, and SmoothStreamingUri will be set if possible.
        /// </summary>
        /// <param name="AssetDetail">JobId is required</param>
        public static void UpdateMediaAsset(MediaAsset AssetDetail)
        {
            if(AssetDetail.JobId!=null && string.IsNullOrEmpty(AssetDetail.SmoothStreamingUri)) { 
                IJob job = mediaContext.Jobs.Where(p => p.Id == AssetDetail.JobId).ToList().FirstOrDefault();
                if (job != null)
                {
                    AssetDetail.JobState = job.State;

                    if(job.State == JobState.Finished)
                    {
                        IAsset encodedAsset = job.OutputMediaAssets[0];
                        if(encodedAsset.Locators.Count()==0) { 
                            mediaContext.Locators.Create(LocatorType.OnDemandOrigin, encodedAsset, AccessPermissions.Read, TimeSpan.FromDays(30));
                            mediaContext.Locators.Create(LocatorType.Sas, encodedAsset, AccessPermissions.Read, TimeSpan.FromDays(30));
                        }

                        AssetDetail.EncodedAssetId = encodedAsset.Id;
                        if(encodedAsset.GetSmoothStreamingUri()!=null) AssetDetail.SmoothStreamingUri = encodedAsset.GetSmoothStreamingUri().AbsoluteUri;

                    }
                }
            }

        }

		public static void DeleteOrCancelJob(string jobId)
		{
            if(!string.IsNullOrEmpty(jobId)) { 
                IJob job = mediaContext.Jobs.Where(p => p.Id == jobId).ToList().FirstOrDefault();
                if (job != null)
                {
                    if (job.State != JobState.Canceled && job.State != JobState.Canceling && job.State != JobState.Error && job.State != JobState.Finished)
                    {
                        job.Cancel();
                    }
                    else
                    {
                        job.Delete();
                    }
                }
            }
		}

        public static void DeleteAsset(string assetId)
        {
            try
            {
                IAsset asset = mediaContext.Assets.Where(p => p.Id == assetId).ToList().FirstOrDefault();

                if (asset != null)
                {
                    foreach (var l in asset.Locators)
                    {
                        l.Delete();
                    }
                    asset.Delete();
                }
            }
            catch { }
        }

        private static Stream ProcessImage(Stream fileStream, string uploadType)
        {
            ISupportedImageFormat format = new JpegFormat { Quality = 90 };

            using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
            {
                Stream imageStream = new MemoryStream();

                imageFactory.Load(fileStream);
                imageFactory.Resize(new ResizeLayer(GetImageSize(uploadType)) { ResizeMode = GetImageCrop(uploadType) });
                imageFactory.Format(format);
                imageFactory.Save(imageStream);

                return imageStream;
            }

        }

        private static Size GetImageSize(string uploadType)
        {
            if (uploadType == "ProfilePhoto" ||
                uploadType == "ArtistImage")
                return new Size(500, 375);

            if (uploadType == "BannerImage")
                return new Size(970, 350);

            if (uploadType == "ContestImage")
                return new Size(640, 210);

            if (uploadType == "AlbumArt")
                return new Size(500, 500);

            return new Size(1200, 1200);
        }

        private static ResizeMode GetImageCrop(string uploadType)
        {
            if (uploadType == "ProfilePhoto" ||
                uploadType == "ArtistImage" ||
                uploadType == "ContestImage")
                return ResizeMode.Max;

            if (uploadType == "BannerImage")
                return ResizeMode.Crop;

            if (uploadType == "AlbumArt")
                return ResizeMode.Crop;

            return ResizeMode.Crop;
        }

        public static bool ValidUploadExtension(string uploadType, HttpPostedFileBase file)
        {
            if (file == null || string.IsNullOrEmpty(file.FileName) || string.IsNullOrEmpty(uploadType))
                return false;

            string extension = Path.GetExtension(file.FileName).ToLower();

            if (uploadType == "ProfilePhoto" ||
                uploadType == "AlbumArt" ||
                uploadType == "ArtistImage" ||
                uploadType == "BannerImage" ||
                uploadType == "ContestImage")
                return extension == ".jpg" ||
                       extension == ".jpeg" ||
                       extension == ".png" ||
                       extension == ".gif" ||
                       extension == ".tif" ||
                       extension == ".bmp";

            if (uploadType == "Audio")
                return extension == ".mp3" ||
                    extension == ".ac3" ||
                    extension == ".aiff" ||
                    extension == ".bwf" ||
                    extension == ".m4a" ||
                    extension == ".m4b" ||
                    extension == ".wav" ||
                    extension == ".wma";

            if (uploadType == "Video")
                return extension == ".3gp" ||
                    extension == ".3g2" ||
                    extension == ".3gp2" ||
                    extension == ".asf" ||
                    extension == ".mts" ||
                    extension == ".m2tf" ||
                    extension == ".avi" ||
                    extension == ".mod" ||
                    extension == ".dv" ||
                    extension == ".ts" ||
                    extension == ".vob" ||
                    extension == ".xesc" ||
                    extension == ".mp4" ||
                    extension == ".mpeg" ||
                    extension == ".mpg" ||
                    extension == ".m2v" ||
                    extension == ".ismv" ||
                    extension == ".wmv";

            return false;
        }

        public static Tuple<string, string> GetContainerReferenceFromURI(string uri)
        {
            int i = uri.LastIndexOf('/');
            int j = uri.LastIndexOf('/', i - 1) + 1;
            string Container = uri.Substring(j, i - j);
            string Reference = uri.Substring(i + 1);
            return Tuple.Create(Container, Reference);
        }

        public static ManageBlobIndexModel ManageBlobs()
        {
            ManageBlobIndexModel result = new ManageBlobIndexModel();
            result.Containers = new List<ManageBlobContainerModel>();

            ManageBlobContainerModel container;
            ManageBlobModel blob;

            foreach (CloudBlobContainer cloudContainer in blobClient.ListContainers())
            {
                container = new ManageBlobContainerModel();
                container.Name = cloudContainer.Name;
                container.Blobs = new List<ManageBlobModel>();

                foreach (CloudBlob cloudBlob in cloudContainer.ListBlobs())
                {
                    blob = new ManageBlobModel();
                    blob.Name = cloudBlob.Name;
                    blob.Delete = false;

                    container.Blobs.Add(blob);
                }

                result.Containers.Add(container);
            }

            return result;
        }
    }
    public class MediaAsset {
        public string FileId { get; set; }
        public string AssetId { get; set; }
        public string JobId { get; set; }
        public JobState JobState { get; set; }
        public string EncodedAssetId { get; set; }
        public string SmoothStreamingUri { get; set; }
		public double TimeEstimate { get; set; }
    }
}