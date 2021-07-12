FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env
WORKDIR /app

# Copy everything and build
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 80/tcp

ENTRYPOINT ["dotnet", "ApiPaises0.1.3.dll"]



#publicando
#dotnet publish --configuration Release --output dist
#FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine
#
#LABEL version="1.0.0" descripion="APIDotNet3.0 Paises Cidades Regioes"
#
#WORKDIR /app
#
#COPY dist /app
#
#EXPOSE 80/tcp
#
#ENTRYPOINT ["dotnet", "ApiPaises0.1.3.dll"]
#