#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8090
EXPOSE 8091

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LiveTrackerSessionAPI/LiveTrackerSessionAPI.csproj", "LiveTrackerSessionAPI/"]
COPY ["SessionAPIModels/SessionAPIModels.csproj", "SessionAPIModels/"]
COPY ["SessionAPICommonModels/SessionAPICommonModels.csproj", "SessionAPICommonModels/"]
RUN dotnet restore "./LiveTrackerSessionAPI/LiveTrackerSessionAPI.csproj"
COPY . .
WORKDIR "/src/LiveTrackerSessionAPI"
RUN dotnet build "./LiveTrackerSessionAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LiveTrackerSessionAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiveTrackerSessionAPI.dll"]