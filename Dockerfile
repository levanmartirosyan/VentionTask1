FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["VentionTask1/VentionTask1.csproj", "VentionTask1/"]
RUN dotnet restore "VentionTask1/VentionTask1.csproj"

COPY . .
WORKDIR /src/VentionTask1
RUN dotnet publish "VentionTask1.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VentionTask1.dll"]
