#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["API/ZODs.Api/ZODs.Api.csproj", "API/ZODs.Api/"]
COPY ["API/ZODs.Api.Repository/ZODs.Api.Repository.csproj", "API/ZODs.Api.Repository/"]
COPY ["API/ZODs.Api.Repository.Entities/ZODs.Api.Repository.Entities.csproj", "API/ZODs.Api.Repository.Entities/"]
COPY ["API/ZODs.Api.Service/ZODs.Api.Service.csproj", "API/ZODs.Api.Service/"]
COPY ["API/ZODs.Api.Identity/ZODs.Api.Identity.csproj", "API/ZODs.Api.Identity/"]
COPY ["API/ZODs.Api.Common/ZODs.Api.Common.csproj", "API/ZODs.Api.Common/"]
COPY ["Common/ZODs.Common/ZODs.Common.csproj", "Common/ZODs.Common/"]
COPY ["Payment/ZODs.Payment/ZODs.Payment.csproj", "Payment/ZODs.Payment/"]
COPY ["AI/ZODs.AI.Common/ZODs.AI.Common.csproj", "AI/ZODs.AI.Common/"]
COPY ["AI/ZODs.AI.OpenAI/ZODs.AI.OpenAI.csproj", "AI/ZODs.AI.OpenAI/"]

RUN dotnet restore "API/ZODs.Api/ZODs.Api.csproj"
COPY . .
WORKDIR "/src/API/ZODs.Api"
RUN dotnet build "ZODs.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZODs.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZODs.Api.dll"]