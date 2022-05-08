FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY ["src/SecuredApi/SecuredApi.fsproj", "/src/SecuredApi/"]
RUN dotnet restore "/src/SecuredApi/SecuredApi.fsproj"
COPY . .
WORKDIR "/src/SecuredApi"
RUN dotnet build "SecuredApi.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SecuredApi.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SecuredApi.dll"]
