FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["SensorCloud/SensorCloud.csproj", "SensorCloud/"]
COPY ["Onkyo/Onkyo.csproj", "Onkyo/"]
COPY ["JvcProjector/JvcProjector.csproj", "JvcProjector/"]
RUN dotnet restore "SensorCloud/SensorCloud.csproj"
COPY . .
WORKDIR "/src/SensorCloud"
RUN dotnet build "SensorCloud.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SensorCloud.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
#COPY config.json /app
COPY eiscp-commands.json /app
ENTRYPOINT ["dotnet", "SensorCloud.dll"]