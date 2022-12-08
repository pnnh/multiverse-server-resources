FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Gliese/Gliese.csproj", "Gliese/"]
RUN dotnet restore "Gliese/Gliese.csproj"
COPY . .
WORKDIR "/src/Gliese"
RUN dotnet build "Gliese.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gliese.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gliese.dll"]
