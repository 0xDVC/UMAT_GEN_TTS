FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UMAT_GEN_TTS/UMAT_GEN_TTS.csproj", "UMAT_GEN_TTS/"]
RUN dotnet restore "UMAT_GEN_TTS/UMAT_GEN_TTS.csproj"
COPY . .
WORKDIR "/src/UMAT_GEN_TTS"
RUN dotnet build "UMAT_GEN_TTS.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UMAT_GEN_TTS.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UMAT_GEN_TTS.dll"]
