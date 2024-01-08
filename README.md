# Localstack
### This is an show case of how to use localstack s3 for local development with asp.net core web api in .net 8. first install localstack and run localstack using pip
```
pip install localstack
localstack start
```
then navigation to this link on browser. localstack will fire up management dashboard for managing your aws service elegently. you can do all things like creating s3 bucket, configure cors, put object, get object and remove object etc.
https://app.localstack.cloud/inst/default/resources

then install aws cli or using awslocal cli
```
pip install awscli
or
pip install awscli-local
```
then configure your aws profile and credentials such as secret key id, access key id, region, and output format.
```
aws configure
or
aws configure --profile [profile name]
```
answer the prompt with the following.
```
access key id: test
secret key id: test
region: us-east-1 # recommended
output: json
```
all done from s3 configuration on localstack side.

# .Net core web api

### for dotnet side. I recommend creating a new dotnet web api project (you should consider using minimal api for testing)

then install nuget package awssdk.s3
```
dotnet add package AWSSDK.S3 --version 3.7.305.6
```
configure appSetting.Development.json with this
```
"AWS": {
    "BucketName": "vitoubucket",
    "Region": "us-east-1",
    "ServiceURL": "http://localhost:4566",
    "ForcePathStyle": "true",
    "AwsAccessKey": "test",
    "AwsSecretKey": "test"
}
```
then configure service like this on program.cs
```
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
```
add a simple endpoint to add a new object to bucket.
```
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
```
### for development only. I use .DisableAntiforgery() to disable antiforgery which is enable by default in file upload of post request in .net core

# Congratulation! 
you now have s3 running on your local machine and access it like majic. Head over to https://app.localstack.cloud/inst/default/resources to see the newly put object in the bucket.




