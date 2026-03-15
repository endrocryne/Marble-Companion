FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["MarbleCompanion.API/MarbleCompanion.API.csproj", "MarbleCompanion.API/"]
COPY ["MarbleCompanion.Shared/MarbleCompanion.Shared.csproj", "MarbleCompanion.Shared/"]
RUN dotnet restore "MarbleCompanion.API/MarbleCompanion.API.csproj"
COPY MarbleCompanion.API/ MarbleCompanion.API/
COPY MarbleCompanion.Shared/ MarbleCompanion.Shared/
WORKDIR "/src/MarbleCompanion.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MarbleCompanion.API.dll"]
