# Usa a imagem oficial do SDK do .NET 9 para construir o projeto
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["system-copsoq-api.csproj", "./"]
RUN dotnet restore "system-copsoq-api.csproj"
COPY . .
RUN dotnet publish "system-copsoq-api.csproj" -c Release -o /app/publish

# Usa a imagem oficial do runtime do .NET 9 para rodar o projeto
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "system-copsoq-api.dll"]