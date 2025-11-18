FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FinanceTracker.Api.csproj .
RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update && \
    apt-get install -y postgresql-client curl && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./
COPY wait-for-db.sh /wait-for-db.sh
RUN chmod +x /wait-for-db.sh

ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80

ENTRYPOINT ["/wait-for-db.sh", "dotnet", "FinanceTracker.Api.dll"]
