# Multi-stage build for ASP.NET Core
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as separate layers for caching
COPY AeroponicIOT.csproj ./
RUN dotnet restore "AeroponicIOT.csproj"

# Copy everything else and publish
COPY . ./
RUN dotnet publish "AeroponicIOT.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "AeroponicIOT.dll"]
