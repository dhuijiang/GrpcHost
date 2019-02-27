# GrpcHost
.Net Generic Host implementation for gRPC

## Configuration

In .NetCore Microsoft provides us with means for loading configuration file through [Options patter](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-2.2)
GrpcHost support the same feature you can use: "hostsettings.json" and extend it or use additional configuration files which ever suite your needs.

The existing configuration covers:
  1. Configuring the host
  2. Configuring logging
  3. Configure client caching

### Configuration - HostOptions

Here you can setup host address and port, if you decide to ommit these values by default grpc server address will be: 0.0.0.0:80 which is probably fine if you are planning to host your grpc server
in docker containers, otherwise you will want to change the default to something more appropriate.

```
"HostOptions": {
	"Host": "localhost",
    "Port": 5000
},
```

TODO: Add supprot for configuring grpc timeouts (deadline).

### Configuration - LoggingOptions

Provides you with means to filter request/response properties are just allow everything to be logged. GrpcHost uses Mircorosfot.Extension.Logging.ILogger as a logging abstraction but the logging 
implementation that's actually being used is Serilog Console or Splunk sink. In the future I might revisit this and remove the Serilog dependency, but logging will still be structured, so traditional
loggers will not offer you with best results.

TODO - Add more details and examples on how logging configuration works.

### Configuration - ChannelOptions

Since grpc channels need to be reused as much as possible, grpc host offers client cacheing. In "ChannelOptions" you can specify the list of grpc clients that the server might consume while processing
requests.

```
"ChannelOptions": [{
    "ServiceName": "CustomerService",
    "Host": "localhost",
    "Port": 5000
}, 
{
    "ServiceName": "ProductService",
    "Host": "localhost",
    "Port": 5005
}]
```

TODO: Currently there is no expiration for cached client, so in case that you need to update the client addres you will need to restart the server, this will be fixed in future, by adding the flag that 
will be used to invalidate the cached clients.

## Healthceks

In Grpc.Core v1.17 we got the health check api [that we can easily implement](https://consoleout.com/2019/01/12/add-health-checks-for-grpc-services.html) GrpcHost offer additional means of overriding
the healthcheck functionality. If you are satisfied with what happens out of the box then you don't have to do anything, otherwise you can implement: IHealthCheckOverrided interface. The interface
conatins only one method: "IsHealthy" that has no input parameters and returns bool value. If return is "true" health status will be "Serving", otherwise it'll be "NotServing".
You will need to register the interface in DI configuration and you are free to add any additional dependencies that you might need for you healthcheck logic.

## DI

TODO

### Registering gRPC methods
#### Registering methods with identical signatures
### Registering interceptors
#### GlobalInterceptor
### What's already registered

## Instrumentation

### Correlation Id
### Tracing - OpenTracing + Jaeger


