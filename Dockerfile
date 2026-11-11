
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files
COPY ["PMS.API/PMS.API.csproj", "PMS.API/"]
COPY ["PMS.Core/PMS.Core.csproj", "PMS.Core/"]
COPY ["PMS.Data/PMS.Data.csproj", "PMS.Data/"]
COPY ["PMS.Service/PMS.Service.csproj", "PMS.Service/"]

# Restore NuGet packages
RUN dotnet restore "PMS.API/PMS.API.csproj"

# Copy remaining source code
COPY . .

# Publish API
RUN dotnet publish "PMS.API/PMS.API.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false


# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "PMS.API.dll"]
