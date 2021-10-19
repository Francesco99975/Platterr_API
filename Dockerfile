FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet tool install --global dotnet-ef --version 5.0.8
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations add InitialCreate -o Data/Migrations
RUN dotnet ef database update
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/platterr_api.db .
COPY --from=build-env /app/out .

EXPOSE 80

ENTRYPOINT ["dotnet", "platterr_api.dll"]