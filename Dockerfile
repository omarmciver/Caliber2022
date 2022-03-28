FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /build
COPY . ./
RUN dotnet restore ./api/api.csproj
RUN dotnet publish ./api/api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /app
COPY --from=builder /build/out .
EXPOSE 5000/tcp
ENTRYPOINT ["dotnet", "api.dll", "--urls=http://0.0.0.0:5000"]