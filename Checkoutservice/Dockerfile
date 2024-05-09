FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Checkoutservice/Checkoutservice.csproj", "Checkoutservice/"]
RUN dotnet restore "Checkoutservice/Checkoutservice.csproj"
COPY . .
WORKDIR "/src/Checkoutservice"
RUN dotnet build "Checkoutservice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Checkoutservice.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Checkoutservice.dll"]
