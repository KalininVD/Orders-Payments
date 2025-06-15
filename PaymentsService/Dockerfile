FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE ${API_PORT}

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["PaymentsService/PaymentsService.Application/PaymentsService.Application.csproj", "PaymentsService/PaymentsService.Application/"]
COPY ["PaymentsService/PaymentsService.Domain/PaymentsService.Domain.csproj", "PaymentsService/PaymentsService.Domain/"]
COPY ["PaymentsService/PaymentsService.Infrastructure/PaymentsService.Infrastructure.csproj", "PaymentsService/PaymentsService.Infrastructure/"]
COPY ["PaymentsService/PaymentsService.Web/PaymentsService.Web.csproj", "PaymentsService/PaymentsService.Web/"]
RUN dotnet restore "PaymentsService/PaymentsService.Web/PaymentsService.Web.csproj"
COPY . .
WORKDIR "/src/PaymentsService/PaymentsService.Web"
RUN dotnet build "PaymentsService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaymentsService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentsService.Web.dll"]