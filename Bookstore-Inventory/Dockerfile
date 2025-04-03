FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Bookstore.API/Bookstore.API.csproj", "Bookstore.API/"]
COPY ["Bookstore.Application/Bookstore.Application.csproj", "Bookstore.Application/"]
COPY ["Bookstore.Domain/Bookstore.Domain.csproj", "Bookstore.Domain/"]
COPY ["Bookstore.Infrastructure/Bookstore.Infrastructure.csproj", "Bookstore.Infrastructure/"]
RUN dotnet restore "Bookstore.API/Bookstore.API.csproj"
COPY . .
WORKDIR "/src/Bookstore.API"
RUN dotnet build "Bookstore.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bookstore.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:10000
ENTRYPOINT ["dotnet", "Bookstore.API.dll"]