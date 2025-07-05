# Sử dụng stage build để tối ưu kích thước image cuối cùng
# Base image cho SDK để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file solution và project files, sau đó khôi phục các dependency
# Điều này giúp tận dụng Docker cache hiệu quả hơn
COPY ["SportManagerService.sln", "."]
COPY ["SportManager.API/SportManager.API.csproj", "SportManager.API/"]
COPY ["SportManager.Application/SportManager.Application.csproj", "SportManager.Application/"]
COPY ["SportManager.Domain/SportManager.Domain.csproj", "SportManager.Domain/"]
COPY ["SportManager.Infrastructure/SportManager.Infrastructure.csproj", "SportManager.Infrastructure/"]

RUN dotnet restore "SportManagerService.sln"

# Copy toàn bộ source code
COPY . .

# Publish ứng dụng API
# Sử dụng `--no-restore` vì chúng ta đã khôi phục ở trên
# Đảm bảo publish output ra thư mục 'out'
WORKDIR /src/SportManager.API
RUN dotnet publish "SportManager.API.csproj" -c Release -o /app/publish --no-restore

# Stage cuối cùng để tạo runtime image nhỏ gọn
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy các file đã publish từ stage 'build'
COPY --from=build /app/publish .

# Định nghĩa điểm vào của ứng dụng
ENTRYPOINT ["dotnet", "SportManager.API.dll"]
