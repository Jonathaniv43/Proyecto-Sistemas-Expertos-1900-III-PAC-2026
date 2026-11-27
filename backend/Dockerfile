# Etapa 1: build — SDK completo, solo existe durante la compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Norvia.slnx
RUN dotnet publish Norvia.API/Norvia.API.csproj -c Release -o /app/publish --no-restore

# Etapa 2: runtime — imagen liviana, lo único que llega a producción
# Etapa 2: runtime — imagen liviana, lo único que llega a producción
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER app
ENTRYPOINT ["dotnet", "Norvia.API.dll"]