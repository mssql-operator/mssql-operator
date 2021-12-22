FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["mssql-operator/mssql-operator.csproj", "mssql-operator/"]
RUN dotnet restore "mssql-operator/mssql-operator.csproj"
COPY . .
RUN dotnet build "mssql-operator/mssql-operator.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "mssql-operator/mssql-operator.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "mssql-operator.dll"]