# Elasticsearch Net for Amazon AWS

Add-on to [elasticsearch-net / NEST](https://github.com/elastic/elasticsearch-net) for using AWS's elasticsearch service.

## Install Package
On Nuget
* Current Version - https://www.nuget.org/packages/Elasticsearch.Net.Aws/
* For NEST / Elasticsearch.Net 2.X - https://www.nuget.org/packages/bcuff.Elasticsearch.Net.Aws-v2/
* For NEST / Elasticsearch.Net 1.X - https://www.nuget.org/packages/Elasticsearch.Net.Aws-v1/

```PowerShell
# For ElasticSearch.Net >= 5.0.0
Install-Package Elasticsearch.Net.Aws
# or for dotnet core
dotnet add package Elasticsearch.Net.Aws

# For ElasticSearch.Net 2.X
Install-Package bcuff.Elasticsearch.Net.Aws-v2
# or for dotnet core
dotnet add package bcuff.Elasticsearch.Net.Aws-v2

# For ElasticSearch.Net 1.X
Install-Package Elasticsearch.Net.Aws-v1
# or for dotnet core
dotnet add package Elasticsearch.Net.Aws-v1
```

## Setup

#### Elasticsearch.Net Version >= 2.0.2

**Use Package Elasticsearch.Net.Aws**

```csharp
// for NEST

// if using app.config, environment variables, or roles
var httpConnection = new AwsHttpConnection("us-east-1");

var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
var config = new ConnectionSettings(pool, httpConnection);
var client = new ElasticClient(config);
```

#### Elasticsearch.Net Version 1.7.1

**Use Package Elasticsearch.Net.Aws-v1**

Source for this version is maintained on the version-1 branch

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

#### Serilog Sink Setup

```csharp
  const string esUrl = "https://aws-es-thinger.us-west-1.es.amazonaws.com";
  Log.Logger = new LoggerConfiguration()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(esUrl))
                {
                    ModifyConnectionSettings = conn =>
                    {
                        var httpConnection = new AwsHttpConnection("us-east-1");
                        var pool = new SingleNodeConnectionPool(new Uri(esUrl));
                        return new ConnectionConfiguration(pool, httpConnection);
                    }
                })
                .CreateLogger();
```
