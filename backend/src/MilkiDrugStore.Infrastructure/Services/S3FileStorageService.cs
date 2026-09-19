using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using System.Net;

namespace MilkiDrugStore.Infrastructure.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<S3FileStorageService> _logger;

    public S3FileStorageService(IConfiguration configuration, ILogger<S3FileStorageService> logger)
    {
        _logger = logger;
        _bucketName = configuration.GetValue<string>("FileStorage:S3:BucketName") ?? string.Empty;
        var region = configuration.GetValue<string>("FileStorage:S3:Region") ?? "us-east-1";
        var accessKeyId = configuration.GetValue<string>("FileStorage:S3:AccessKeyId") ?? string.Empty;
        var secretAccessKey = configuration.GetValue<string>("FileStorage:S3:SecretAccessKey") ?? string.Empty;

        var s3Config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(region) };

        if (!string.IsNullOrWhiteSpace(accessKeyId) && !string.IsNullOrWhiteSpace(secretAccessKey))
        {
            _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, s3Config);
        }
        else
        {
            _s3Client = new AmazonS3Client(s3Config);
        }
    }

    public S3FileStorageService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3FileStorageService> logger)
    {
        _s3Client = s3Client;
        _bucketName = configuration.GetValue<string>("FileStorage:S3:BucketName") ?? string.Empty;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream stream, string fileName, string? contentType = null)
    {
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName))
            safeName = $"{Guid.NewGuid()}.bin";

        var subfolder = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var key = $"{subfolder}/{Guid.NewGuid():N}_{safeName}";

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            BucketName = _bucketName,
            Key = key,
            ContentType = contentType,
            AutoResetStreamPosition = true
        };

        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(uploadRequest);

        _logger.LogInformation("File uploaded to S3 bucket {Bucket} with key {Key}", _bucketName, key);
        return key;
    }

    public async Task<Stream?> GetFileAsync(string key)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(_bucketName, key);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("S3 object not found: {Bucket}/{Key}", _bucketName, key);
            return null;
        }
    }

    public async Task<bool> FileExistsAsync(string key)
    {
        try
        {
            await _s3Client.GetObjectMetadataAsync(_bucketName, key);
            return true;
        }
        catch (AmazonS3Exception ex) when (
            ex.StatusCode == HttpStatusCode.NotFound ||
            ex.StatusCode == (HttpStatusCode)404)
        {
            return false;
        }
    }

    public async Task DeleteFileAsync(string key)
    {
        await _s3Client.DeleteObjectAsync(_bucketName, key);
        _logger.LogInformation("Deleted S3 object: {Bucket}/{Key}", _bucketName, key);
    }
}
