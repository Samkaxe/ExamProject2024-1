FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Inventoryservice/Inventoryservice.csproj", "Inventoryservice/"]
RUN dotnet restore "Inventoryservice/Inventoryservice.csproj"
COPY . .
WORKDIR "/src/Inventoryservice"
RUN dotnet build "Inventoryservice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Inventoryservice.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inventoryservice.dll"]
