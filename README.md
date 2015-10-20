# Elasticsearch Net for Amazon AWS

Add-on to [elasticsearch-net / NEST](https://github.com/elastic/elasticsearch-net) for using AWS's elasticsearch service.

## Install Package
```
Install-Package Elasticsearch.Net.Aws
```

## Setup
```csharp
// for NEST
var client = new ElasticClient(settings, connection: new AwsHttpConnection(settings, new AwsSettings
{
	AccessKey = "My AWS access key",
	SecretKey = "My AWS secret key",
	Region = "us-east-1",
}));
```

The `AwsHttpConnection` class is an implemenation of `IConnection` that will sign the HTTP requests according to the [Version 4 Signing Process](http://docs.aws.amazon.com/general/latest/gr/signature-version-4.html).
