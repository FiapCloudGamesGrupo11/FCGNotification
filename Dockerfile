FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY NotificationsAPI/NotificationsAPI.csproj NotificationsAPI/

RUN dotnet restore "NotificationsAPI/NotificationsAPI.csproj"

COPY . .

WORKDIR /src/NotificationsAPI

RUN dotnet publish "NotificationsAPI.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NotificationsAPI.dll"]