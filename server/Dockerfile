FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SolidTradeServer.csproj", "./"]
RUN dotnet restore "SolidTradeServer.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "SolidTradeServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SolidTradeServer.csproj" -c Release -o /app/publish

FROM base AS final
ARG ENVIRONMENT
ENV ASPNETCORE_ENVIRONMENT ${ENVIRONMENT}

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SolidTradeServer.dll"]
