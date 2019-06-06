FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["SensorCloud/SensorCloud.csproj", "SensorCloud/"]
COPY ["Onkyo/Onkyo.csproj", "Onkyo/"]
COPY ["JvcProjector/JvcProjector.csproj", "JvcProjector/"]
COPY ["P1Meter/P1Meter.csproj", "P1Meter/"]
COPY ["NNTP/NNTP.csproj", "NNTP/"]
COPY ["SpotNet/SpotNet.csproj", "SpotNet/"]
RUN dotnet restore "SensorCloud/SensorCloud.csproj"
COPY . .
WORKDIR "/src/SensorCloud"
RUN dotnet build "SensorCloud.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SensorCloud.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
#COPY config.json /app/
COPY eiscp-commands.json /app
COPY SensorCloud/wwwroot /app/SensorCloud/wwwroot
RUN apt-get update
RUN apt-get install -y --no-install-recommends libc6-dev libgdiplus
ENTRYPOINT ["dotnet", "SensorCloud.dll"]