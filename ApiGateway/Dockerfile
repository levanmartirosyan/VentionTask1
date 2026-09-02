FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
COPY ["VentionTask1.Application/VentionTask1.Application.csproj", "VentionTask1.Application/"]
COPY ["VentionTask1.Domain/VentionTask1.Domain.csproj", "VentionTask1.Domain/"]

RUN dotnet restore "ApiGateway/ApiGateway.csproj"

COPY . .

WORKDIR /src/ApiGateway
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ApiGateway.dll"]