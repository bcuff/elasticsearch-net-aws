# Elasticsearch Net for Amazon AWS

Add-on to [elasticsearch-net / NEST](https://github.com/elastic/elasticsearch-net) for using AWS's elasticsearch service.

## Install Package
```PowerShell
# For ElasticSearch.Net >= 2.0.2
Install-Package Elasticsearch.Net.Aws

# For ElasticSearch.Net = 1.7.1
Install-Package Elasticsearch.Net.Aws-v1
```

## Setup

#### Elasticsearch.Net Version >= 2.0.2

**Use Package Elasticsearch.Net.Aws**

```csharp
// for NEST

// if using an access key
var httpConnection = new AwsHttpConnection(new AwsSettings
{
	AccessKey = "My AWS access key",
	SecretKey = "My AWS secret key",
	Region = "us-east-1",
});

// if using roles
var httpConnection = new AwsHttpConnection("us-east-1");

var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
var config = new ConnectionSettings(pool, httpConnection);
var client = new ElasticClient(config);
```

#### Elasticsearch.Net Version 1.7.1

**Use Package Elasticsearch.Net.Aws-v1**
**Source for this version is maintained on the version-1 branch**

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
