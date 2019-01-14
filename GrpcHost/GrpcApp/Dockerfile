FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY GrpcApp/GrpcServer.csproj GrpcApp/
COPY GrpcHost/GrpcHost.csproj GrpcHost/
COPY CustomerGrpcService/CustomerGrpcService.csproj CustomerGrpcService/
COPY Contracts/Contracts.csproj Contracts/
COPY Services/Services.csproj Services/
COPY ProductGrpcService/ProductGrpcService.csproj ProductGrpcService/
RUN dotnet restore GrpcApp/GrpcServer.csproj
COPY . .
WORKDIR /src/GrpcApp
RUN dotnet build GrpcServer.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish GrpcServer.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "GrpcServer.dll"]
