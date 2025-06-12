FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE ${API_PORT}

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PaymentsService/PaymentsService.Web/PaymentsService.Web.csproj", "PaymentsService.Web/"]
COPY ["PaymentsService/PaymentsService.Application/PaymentsService.Application.csproj", "PaymentsService.Application/"]
COPY ["PaymentsService/PaymentsService.Domain/PaymentsService.Domain.csproj", "PaymentsService.Domain/"]
COPY ["PaymentsService/PaymentsService.Infrastructure/PaymentsService.Infrastructure.csproj", "PaymentsService.Infrastructure/"]
RUN dotnet restore "PaymentsService.Web/PaymentsService.Web.csproj"
COPY PaymentsService/PaymentsService.Web/ ./PaymentsService.Web/
COPY PaymentsService/PaymentsService.Application/ ./PaymentsService.Application/
COPY PaymentsService/PaymentsService.Domain/ ./PaymentsService.Domain/
COPY PaymentsService/PaymentsService.Infrastructure/ ./PaymentsService.Infrastructure/
WORKDIR "/src/PaymentsService.Web"
RUN dotnet build "PaymentsService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaymentsService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentsService.Web.dll"]