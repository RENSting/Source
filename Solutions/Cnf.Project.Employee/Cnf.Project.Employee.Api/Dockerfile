FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Cnf.Project.Employee.Api/Cnf.Project.Employee.Api.csproj", "Cnf.Project.Employee.Api/"]
RUN dotnet restore "Cnf.Project.Employee.Api/Cnf.Project.Employee.Api.csproj"
COPY . .
WORKDIR "/src/Cnf.Project.Employee.Api"
RUN dotnet build "Cnf.Project.Employee.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cnf.Project.Employee.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cnf.Project.Employee.Api.dll"]