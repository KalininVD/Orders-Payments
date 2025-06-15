FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE ${API_PORT}

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["OrdersService/OrdersService.Application/OrdersService.Application.csproj", "OrdersService/OrdersService.Application/"]
COPY ["OrdersService/OrdersService.Domain/OrdersService.Domain.csproj", "OrdersService/OrdersService.Domain/"]
COPY ["OrdersService/OrdersService.Infrastructure/OrdersService.Infrastructure.csproj", "OrdersService/OrdersService.Infrastructure/"]
COPY ["OrdersService/OrdersService.Web/OrdersService.Web.csproj", "OrdersService/OrdersService.Web/"]
RUN dotnet restore "OrdersService/OrdersService.Web/OrdersService.Web.csproj"
COPY . .
WORKDIR "/src/OrdersService/OrdersService.Web"
RUN dotnet build "OrdersService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OrdersService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrdersService.Web.dll"]