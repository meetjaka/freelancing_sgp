# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SGP_Freelancing.csproj", "."]
RUN dotnet restore "./SGP_Freelancing.csproj"
COPY . .
RUN dotnet build "./SGP_Freelancing.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "./SGP_Freelancing.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SGP_Freelancing.dll"]