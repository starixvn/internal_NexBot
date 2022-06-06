FROM registry.nexdev.net:5050/docker/dotnet/core/sdk:5.0.102-ca-patch-buster-slim AS builder

WORKDIR /src

COPY ["NexBot.Webhook/NexBot.csproj", "NexBot.Webhook/"]
COPY ["NexBot.Services/NexBot.Services.csproj", "NexBot.Services/"]
COPY ["NexBot.Criteria/NexBot.Criteria.csproj", "NexBot.Criteria/"]
COPY ["NexBot.Models/NexBot.Models.csproj", "NexBot.Models/"]
COPY ["NexBot.Core/NexBot.Core.csproj", "NexBot.Core/"]
COPY ["nuget.config", "."]

WORKDIR "/src/NexBot.Webhook"
RUN dotnet restore -v n

COPY ./NexBot.Webhook .

RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM registry.nexdev.net:5050/docker/dotnet/core/aspnet:5.0-buster-slim AS base

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "NexBot.dll"]