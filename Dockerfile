# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy csproj and restore as distinct layers
COPY *.sln ./
COPY BusinessObject/*.csproj ./BusinessObject/
COPY DataAccess/*.csproj ./DataAccess/
COPY Repository/*.csproj ./Repository/
COPY CakeCurious-API/*.csproj ./CakeCurious-API/
RUN dotnet restore

# Copy everything else and build
COPY BusinessObject/. ./BusinessObject/
COPY DataAccess/. ./DataAccess/
COPY Repository/. ./Repository/
COPY CakeCurious-API/. ./CakeCurious-API/
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build-env /app/out .
CMD echo "$FIREBASE_SECRET" > cake-curious-firebase-adminsdk-if0lw-7fa994f0ac.json && dotnet CakeCurious-API.dll && fg