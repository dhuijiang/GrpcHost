# GrpcHost
.Net Generic Host implementation for gRPC

Requirements:
1. Host and Port should be resolvable throgh: IOptions<T> or through "args"
2. Basic HealthChecks must be available for all services, but also the means to override the HealthCheck behavior should be provided
3. Easy ServerServiceDefinition registration trhough DI
4. Way to apply interceptor(s) to a specific ServerServiceDefinition
5. Way to register global interceptor(s) that will be automatically applied to all registered ServerServiceDefinitions. Global interceptor
  should be executed after the service specific interceptors, e.g. if you only have one global interceptor that does error handling it and have several interecptors that are specific to some service the global interceptor will get executed last.
6. Offer a mechanism for setting correlation, so that log message and request can be connected.
