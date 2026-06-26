FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["VentionTask1/VentionTask1.WebApi.csproj", "VentionTask1/"]
COPY ["VentionTask1.Application/VentionTask1.Application.csproj", "VentionTask1.Application/"]
COPY ["VentionTask1.Domain/VentionTask1.Domain.csproj", "VentionTask1.Domain/"]
COPY ["VentionTask1.Infrastructure/VentionTask1.Infrastructure.csproj", "VentionTask1.Infrastructure/"]
RUN dotnet restore "VentionTask1/VentionTask1.WebApi.csproj"

COPY . .
WORKDIR /src/VentionTask1
RUN dotnet publish "VentionTask1.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VentionTask1.WebApi.dll"]
