# Stage 1: Base Runtime Setup
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5001
# Force Kestrel inside the container to listen on Port 5001 to match Nginx
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

# Stage 2: SDK Build Environment
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all project definition files (.csproj) preserving folder structure for caching
COPY ["Agentwork.Api/Agentwork.Api.csproj", "Agentwork.Api/"]
COPY ["Agentwork.Application/Agentwork.Application.csproj", "Agentwork.Application/"]
COPY ["Agentwork.Domain/Agentwork.Domain.csproj", "Agentwork.Domain/"]
COPY ["Agentwork.Infrastructure/Agentwork.Infrastructure.csproj", "Agentwork.Infrastructure/"]

# Restore dependencies across the solution layers
RUN dotnet restore "Agentwork.Api/Agentwork.Api.csproj"

# Copy the rest of the source code files
COPY . .
WORKDIR "/src/Agentwork.Api"

# Compile the Api layer in Release mode
RUN dotnet build "Agentwork.Api.csproj" -c Release -o /app/build

# Stage 3: Publish Compilation 
FROM build AS publish
RUN dotnet publish "Agentwork.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Production Image Assembly
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Explicit entry point executing your DLL
ENTRYPOINT ["dotnet", "Agentwork.Api.dll"]