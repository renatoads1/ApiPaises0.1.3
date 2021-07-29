FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env
WORKDIR /app

# Copy everything else and build
COPY . ./
ENV NUGET_XMLDOC_MODE=none
RUN dotnet nuget add source /app/nuget --name VMINuget
RUN dotnet restore
RUN dotnet publish -c Release -o out
RUN rm -rf /app/nuget

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 80/tcp

ENTRYPOINT ["dotnet", "Calibration.Api.dll"]