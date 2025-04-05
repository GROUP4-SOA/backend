# Quản lý Cửa hàng Sách (Bookstore Inventory)

## Giới thiệu
Hệ thống Quản lý Cửa hàng Sách là một ứng dụng web được phát triển bằng .NET 8 Minimal API, MongoDB và kiến trúc phân lớp. Hệ thống hỗ trợ các chức năng quản lý sách, danh mục, kho hàng và người dùng.

## Công nghệ sử dụng
- .NET 8.0
- C# 12.0
- MongoDB
- HTML/CSS/JavaScript
- Swagger/OpenAPI
- Docker

## Kiến trúc hệ thống
### Kiến trúc phân lớp
1. API Layer
- Minimal API endpoints
- Request/Response handling
- Route configuration

2. Application Layer  
- Business logic
- Services
- DTOs
- Interfaces

3. Infrastructure Layer
- Data access
- Repositories
- Database context
- External services

4. Domain Layer
- Business entities
- Domain logic
- Interfaces

### Dependency Injection
- Registered services
- Repository pattern
- Singleton lifecycle

## Tính năng
### 1. Quản lý Sách
- Xem danh sách sách
- Thêm sách mới
- Cập nhật thông tin sách
- Xóa sách
- Tìm kiếm theo danh mục

### 2. Quản lý Danh mục
- Xem danh sách danh mục
- Thêm danh mục mới 
- Cập nhật danh mục
- Xóa danh mục
- Phân cấp danh mục

### 3. Quản lý Kho
- Nhập kho
- Xuất kho
- Xem lịch sử nhập/xuất
- Báo cáo tồn kho

### 4. Quản lý Người dùng
- Đăng nhập
- Tạo tài khoản mới
- Cập nhật thông tin
- Vô hiệu hóa tài khoản
- Phân quyền người dùng

## API Endpoints

### Quản lý Sách
```http
GET /api/books
GET /api/books/{bookId}
POST /api/books
PUT /api/books/{bookId}
DELETE /api/books/{bookId}
GET /api/books/category/{categoryId}
```

### Quản lý Danh mục
```http
GET /api/categories
GET /api/categories/{categoryId}
POST /api/categories
PUT /api/categories/{categoryId}
DELETE /api/categories/{categoryId}
```

### Xác thực và Phân quyền
```http
POST /api/auth/login
POST /api/users
GET /api/auth/users
PUT /api/auth/{userId}
PUT /api/auth/deactivate/{userId}
```

### Quản lý Kho
```http
GET /api/warehouse-exports
POST /api/warehouse-exports
GET /api/imports
POST /api/imports
```

## Cấu trúc Database
### Collections
- Book
- Category  
- User
- Admin
- Staff
- WarehouseExport
- WarehouseExportBook
- WarehouseImport 
- WarehouseImportBook

## Cài đặt và Triển khai

### Yêu cầu hệ thống
- .NET SDK 8.0 trở lên
- MongoDB
- Docker (tùy chọn)

### Cài đặt Local
```bash
# Clone repository
git clone [repository-url]

# Restore packages
dotnet restore

# Run application
dotnet run
```

### Cấu hình
Chỉnh sửa file appsettings.json:
```json
{
    "DatabaseSettings": {
        "ConnectionString": "mongodb connection string",
        "DatabaseName": "BookstoreDB"
    }
}
```

### Docker
```bash
# Build image
docker build -t bookstore-api .

# Run container
docker run -p 8080:80 bookstore-api
```

## Testing
- Unit Tests
- Integration Tests  
- API Tests
- End-to-end Tests

## Bảo mật
- Basic Authentication
- Role-based Authorization
- Input Validation
- Error Handling
- CORS Policy

## Hiệu năng
- MongoDB Indexing
- Response Caching
- Asynchronous Operations
- Connection Pooling

## Logging và Giám sát
- Console Logging
- Error Tracking
- Performance Metrics
- Audit Trails

## Quy trình phát triển
- Git Flow
- Code Review
- Continuous Integration
- Automated Testing

## Tài liệu liên quan
- API Documentation
- Database Schema
- Deployment Guide
- User Manual

## Xử lý lỗi thường gặp
1. Database Connection
- Kiểm tra connection string
- Xác nhận MongoDB service
- Network connectivity

2. Authentication
- Token validation
- Role permissions
- Session management

3. Business Logic
- Data validation
- Business rules
- State management

## Hướng dẫn đóng góp
1. Fork repository
2. Create feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

## Phiên bản
### v1.0.0
- Tính năng cơ bản
- CRUD operations
- Basic authentication

### v1.1.0 (Upcoming)
- Advanced reporting
- Performance improvements
- Enhanced security

## Người đóng góp
- Ngoc Huynh - Lead Developer

## License
MIT License

## Liên hệ
- Email: [email]
- GitHub: [github-profile]

---
*Cập nhật lần cuối: Tháng 3/2024*
