#Configuration of the .NET sdk environment

FROM mcr.microsoft.com/dotnet/sdk:10.0 as build

WORKDIR /src

COPY . .

RUN dotnet restore "./HealthManager.csproj"

RUN dotnet publish "./HealthManager.csproj" -c "Release" -o "/app/publish"



#Configuration of the asp.net core environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0 as run 

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "HealthManager.dll"]
