using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Ruddy.WEB.Services
{
    public class MediaStorageService : IMediaStorageService, IDisposable
    {
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        private readonly IAmazonS3 _s3Client;
        private readonly RegionEndpoint _bucketRegion;
        private readonly string _bucketName;
        private readonly TransferUtility _transferUtility;
        private readonly string _bucketEndpoint;

        public MediaStorageService(string awsAccessKeyId, string awsSecretAccessKey, string bucketName)
        {
            _bucketRegion = RegionEndpoint.EUCentral1;
            
            _s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, _bucketRegion);

            _bucketName = bucketName;

            _bucketEndpoint = $"https://{bucketName}.{_bucketRegion.GetEndpointForService("s3").Hostname}/";

            _transferUtility = new TransferUtility(_s3Client);
        }


        public Task DeleteMediaAsync(Uri uri)
        {
            throw new NotImplementedException();
        }


        public async Task<Uri> SaveMediaAsync(IFormFile mediaFile)
        {
            if (mediaFile != null)
            {
                string extension = Path.GetExtension(mediaFile.FileName);
                string fileName = Guid.NewGuid().ToString("N").ToString() + extension;
                using (var fileStream = new MemoryStream())
                {
                    mediaFile.CopyTo(fileStream);
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = _bucketName,
                        InputStream = fileStream,
                        Key = fileName,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    await _transferUtility.UploadAsync(fileTransferUtilityRequest);
                }

                return new Uri($"{_bucketEndpoint}{fileName}");
            }
            else
            {
                throw new ArgumentNullException(nameof(mediaFile));
            }
        }

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                // free managed resources
                _s3Client.Dispose();
                _transferUtility.Dispose();
            }

            // free native resources if there are any.
            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }

            isDisposed = true;
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources, but leave the other methods
        // exactly as they are.
        ~MediaStorageService()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
    }
}
