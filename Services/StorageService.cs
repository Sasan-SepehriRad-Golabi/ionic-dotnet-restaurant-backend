using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Ruddy.WEB.Services;
public class StorageService : IStorageService
{
    private readonly IConfiguration _config;

    public StorageService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<S3ResponseDto> UploadFileAsync(S3Object obj, AwsCredentials awsCredentialsValues)
    {
        //var awsCredentialsValues = _config.ReadS3Credentials();

        Console.WriteLine($"Key: {awsCredentialsValues.AccessKey}, Secret: {awsCredentialsValues.SecretKey}");

        var credentials = new BasicAWSCredentials(_config["AWS_Keys:accessKey"], _config["AWS_Keys:secretAccessKeys"]);

        var config = new AmazonS3Config()
        {
            RegionEndpoint = Amazon.RegionEndpoint.EUCentral1
        };

        var response = new S3ResponseDto();
        try
        {
            var uploadRequest = new TransferUtilityUploadRequest()
            {
                InputStream = obj.InputStream,
                Key = obj.Name,
                BucketName = obj.BucketName,
                CannedACL = S3CannedACL.NoACL
            };

            // initialise client
            using var client = new AmazonS3Client(credentials, config);

            // initialise the transfer/upload tools
            var transferUtility = new TransferUtility(client);

            // initiate the file upload
            await transferUtility.UploadAsync(uploadRequest);

            response.StatusCode = 201;
            response.Message = $"{obj.Name} has been uploaded sucessfully";
        }
        catch (AmazonS3Exception s3Ex)
        {
            response.StatusCode = (int)s3Ex.StatusCode;
            response.Message = s3Ex.Message;
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            response.Message = ex.Message;
        }

        return response;
    }

    Task<S3ResponseDto> IStorageService.UploadFileAsync(S3Object obj, AwsCredentials awsCredentialsValues)
    {
        throw new NotImplementedException();
    }
}