FROM hub.nexdev.net/base/dotnet/core/sdk:6.0-alpine AS builder

WORKDIR /src

COPY ["nuget.config", "."]
COPY ["NexBot.Webhook/NexBot.csproj", "NexBot.Webhook/"]
COPY ["NexBot.Services/NexBot.Services.csproj", "NexBot.Services/"]
COPY ["NexBot.Criteria/NexBot.Criteria.csproj", "NexBot.Criteria/"]
COPY ["NexBot.Models/NexBot.Models.csproj", "NexBot.Models/"]
COPY ["NexBot.Core/NexBot.Core.csproj", "NexBot.Core/"]

RUN --mount=type=cache,target=/root/.nuget/packages \
dotnet restore "NexBot.Webhook/NexBot.csproj"

COPY . .

WORKDIR "/src/NexBot.Webhook"

RUN --mount=type=cache,target=/root/.nuget/packages \
dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:6.0-alpine AS base

WORKDIR /app

COPY --from=builder /app .
ENV TZ="Asia/Bangkok"
ENTRYPOINT ["dotnet", "NexBot.dll"]