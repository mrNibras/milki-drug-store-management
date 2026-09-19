namespace MilkiDrugStore.Api.Configuration;

public class FileStorageOptions
{
    public string Provider { get; set; } = "local";
    public LocalFileStorageOptions? Local { get; set; }
    public S3FileStorageOptions? S3 { get; set; }
}

public class LocalFileStorageOptions
{
    public string BasePath { get; set; } = "/var/data/uploads";
}

public class S3FileStorageOptions
{
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
}
