using System.Threading.Tasks;

namespace Ruddy.WEB.Services;

public interface IStorageService
{
    Task<S3ResponseDto> UploadFileAsync(S3Object obj, AwsCredentials awsCredentialsValues);
}