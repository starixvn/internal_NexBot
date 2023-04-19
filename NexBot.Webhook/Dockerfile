FROM hub.nexdev.net/base/dotnet/core/sdk:5.0.102-ca-patch-buster-slim AS builder

WORKDIR /src

COPY ["nuget.config", "."]
COPY ["NexBot.Webhook/NexBot.csproj", "NexBot.Webhook/"]
COPY ["NexBot.Services/NexBot.Services.csproj", "NexBot.Services/"]
COPY ["NexBot.Criteria/NexBot.Criteria.csproj", "NexBot.Criteria/"]
COPY ["NexBot.Models/NexBot.Models.csproj", "NexBot.Models/"]
COPY ["NexBot.Core/NexBot.Core.csproj", "NexBot.Core/"]

RUN dotnet restore "NexBot.Webhook/NexBot.csproj"

COPY . .

WORKDIR "/src/NexBot.Webhook"

RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:5.0-buster-slim AS base

WORKDIR /app

RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf

COPY --from=builder /app .
ENV TZ="Asia/Bangkok"
ENTRYPOINT ["dotnet", "NexBot.dll"]