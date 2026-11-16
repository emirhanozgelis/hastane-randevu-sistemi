FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/HospitalAppointment.API/HospitalAppointment.API.csproj", "src/HospitalAppointment.API/"]
COPY ["src/HospitalAppointment.Application/HospitalAppointment.Application.csproj", "src/HospitalAppointment.Application/"]
COPY ["src/HospitalAppointment.Infrastructure/HospitalAppointment.Infrastructure.csproj", "src/HospitalAppointment.Infrastructure/"]
COPY ["src/HospitalAppointment.Domain/HospitalAppointment.Domain.csproj", "src/HospitalAppointment.Domain/"]
RUN dotnet restore "src/HospitalAppointment.API/HospitalAppointment.API.csproj"
COPY . .
WORKDIR "/src/src/HospitalAppointment.API"
RUN dotnet build "HospitalAppointment.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HospitalAppointment.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalAppointment.API.dll"]
