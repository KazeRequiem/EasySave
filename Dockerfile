FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .

WORKDIR /src/EasySave
RUN dotnet publish "EasySave.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EasySave.dll"]

# docker build -t easysave-app .
# docker run -it --rm easysave-app