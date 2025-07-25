FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /src

COPY WebhookUtility.sln ./
COPY WebhookUtility.Web/WebhookUtility.Web.csproj ./WebhookUtility.Web/
RUN dotnet restore WebhookUtility.sln

COPY . .

RUN dotnet build -c Release -o /app WebhookUtility.sln

FROM builder AS publish
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebhookUtility.Web.dll"]