# Usar la imagen base de .NET Core
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Usar la imagen base de SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ProducerMessage/ProducerMessage/ProducerMessage.csproj", "ProducerMessage/ProducerMessage/"]
COPY ["Queues/Queues.csproj", "Queues/"]
RUN dotnet restore "ProducerMessage/ProducerMessage/ProducerMessage.csproj"
COPY . .
WORKDIR "/src/ProducerMessage"
RUN dotnet build "ProducerMessage/ProducerMessage.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProducerMessage/ProducerMessage.csproj" -c Release -o /app/publish

# Copiar la aplicación publicada a la imagen base de ASP.NET Core
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProducerMessage.dll"]
