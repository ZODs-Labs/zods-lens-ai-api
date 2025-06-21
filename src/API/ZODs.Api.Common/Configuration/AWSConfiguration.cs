namespace ZODs.Api.Common.Configuration;

public sealed class AWSConfiguration
{
    public string AccessKey { get; set; }

    public string SecretKey { get; set; }

    public string Region { get; set; }

    public string LogGroup { get; set; }

    public void ValidateConfiguration(bool isProducation = false)
    {
        if (string.IsNullOrWhiteSpace(Region))
        {
            throw new ArgumentException("AWS region is not set.");
        }

        if (isProducation)
        {
            if (string.IsNullOrWhiteSpace(AccessKey))
            {
                throw new ArgumentException("AWS access key is not set.");
            }

            if (string.IsNullOrWhiteSpace(SecretKey))
            {
                throw new ArgumentException("AWS secret key is not set.");
            }

            if (string.IsNullOrWhiteSpace(LogGroup))
            {
                throw new ArgumentException("AWS log group is not set.");
            }
        }
    }
}