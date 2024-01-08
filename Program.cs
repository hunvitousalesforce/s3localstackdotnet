using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;
var awsConfiguration = configuration.GetSection("AWS");
var config = new AmazonS3Config()
    {
        RegionEndpoint = Amazon.RegionEndpoint.USEast1,
        ServiceURL = awsConfiguration.GetValue<string>("ServiceURL"),
        ForcePathStyle = true,
    };
builder.Services.AddSingleton<IAmazonS3>(p =>
    new AmazonS3Client(awsConfiguration.GetValue<string>("AwsAccessKey"), awsConfiguration.GetValue<string>("AwsSecretKey"), config)
);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();


app.MapPost("/upload-image", async (IFormFile file, IAmazonS3 client)=> {
    var bucketName = awsConfiguration.GetValue<string>("BucketName");
    var bucketExisted = await AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);
    if (!bucketExisted) {
        var newBucket = new PutBucketRequest()
        {
            BucketName = bucketName,
            UseClientRegion = true
        };
        await client.PutBucketAsync(newBucket);
    }
    var fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + file.FileName;
    var newObject = new PutObjectRequest()
    {
        BucketName = bucketName,
        Key = fileName,
        InputStream = file.OpenReadStream()
    };
    var response = await client.PutObjectAsync(newObject);
}).DisableAntiforgery();

app.Run(); 