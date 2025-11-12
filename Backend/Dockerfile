# ===============================
# Stage 1: Build and publish .NET API
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src


# Copy the solution file and restore dependencies
COPY Backend.sln ./
COPY API/*.csproj ./API/
RUN dotnet restore Backend.sln

# Copy everything else and publish
COPY . .
WORKDIR /src/API
RUN dotnet publish -c Release -o /app/publish

# ===============================
# Stage 2: Runtime
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .

# Expose the port your API runs on
EXPOSE 8080

ENTRYPOINT ["dotnet", "API.dll"]
