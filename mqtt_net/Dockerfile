FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln ./
COPY SensorCloud/ ./SensorCloud/
COPY Onkyo/ ./Onkyo/
COPY JvcProjector/ ./JvcProjector/


RUN dotnet restore SensorCloud.sln

# Copy everything else and build
RUN dotnet publish -c Release -o out SensorCloud.sln

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/SensorCloud/out/*.dll ./
COPY --from=build-env /app/JvcProjector/out/*.dll ./
COPY --from=build-env /app/Onkyo/out/*.dll ./

COPY --from=build-env /app/SensorCloud/out/*.json ./
COPY --from=build-env /app/JvcProjector/out/*.json ./
COPY --from=build-env /app/Onkyo/out/*.json ./

ENTRYPOINT ["dotnet", "SensorCloud.dll"]