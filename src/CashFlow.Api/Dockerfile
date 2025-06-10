FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8090
ENV ASPNETCORE_URLS=http://+:8090

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

RUN mkdir -p /vsdbg
RUN if [ "$BUILD_CONFIGURATION" = "Debug" ]; then \
    apt-get update && curl -sSL https://aka.ms/getvsdbgsh | bash \
    /dev/stdin -v latest -l /vsdbg; fi

WORKDIR /source
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/CashFlow.Api/CashFlow.Api.csproj", "src/CashFlow.Api/"]
RUN dotnet restore "src/CashFlow.Api/CashFlow.Api.csproj"
COPY src/ src/
WORKDIR /source/src/CashFlow.Api
RUN dotnet build "CashFlow.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CashFlow.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /vsdbg /vsdbg
ENTRYPOINT ["dotnet", "CashFlow.Api.dll"]