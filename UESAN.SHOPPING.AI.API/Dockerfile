# Imagen del servicio de IA. Se construye desde la raíz del repositorio:
#   docker compose --profile api up -d --build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY UESAN.SHOPPING.AI/UESAN.SHOPPING.AI.csproj UESAN.SHOPPING.AI/
COPY UESAN.SHOPPING.AI.API/UESAN.SHOPPING.AI.API.csproj UESAN.SHOPPING.AI.API/
RUN dotnet restore UESAN.SHOPPING.AI.API/UESAN.SHOPPING.AI.API.csproj
COPY UESAN.SHOPPING.AI/ UESAN.SHOPPING.AI/
COPY UESAN.SHOPPING.AI.API/ UESAN.SHOPPING.AI.API/
RUN dotnet publish UESAN.SHOPPING.AI.API/UESAN.SHOPPING.AI.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "UESAN.SHOPPING.AI.API.dll"]
