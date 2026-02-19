# Paytak - ASP.NET Core 9.0
# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Paytak/Paytak.csproj", "Paytak/"]
RUN dotnet restore "Paytak/Paytak.csproj"

COPY . .
WORKDIR "/src/Paytak"
RUN dotnet publish "Paytak.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Paytak.dll"]
