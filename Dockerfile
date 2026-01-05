# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src
COPY ./DbProvisioner.csproj .
RUN dotnet restore

COPY ./ .

RUN dotnet publish -c Release -o /app

# --- Final Stage ---
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine

WORKDIR /app

COPY --from=build /app .

CMD ["dotnet", "DbProvisioner.dll"]