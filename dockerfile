FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["MovieReservationAPI.sln", "./"]
COPY ["MovieReservationAPI/MovieReservationAPI.csproj", "MovieReservationAPI/"]
COPY ["BusinessLayer/BusinessLayer.csproj", "BusinessLayer/"]
COPY ["DataAccessLayer/DataAccessLayer.csproj", "DataAccessLayer/"]
COPY ["EntityLayer/EntityLayer.csproj", "EntityLayer/"]
COPY ["DtoLayer/DtoLayer.csproj", "DtoLayer/"]

RUN dotnet restore MovieReservationAPI/MovieReservationAPI.csproj

COPY . .
WORKDIR "/src/MovieReservationAPI"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "MovieReservationAPI.dll"]