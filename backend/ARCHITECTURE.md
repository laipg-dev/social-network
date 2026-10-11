# Kiến trúc backend Social Network

## Mô hình đang sử dụng

Backend không dùng MVC cổ điển kiểu đặt toàn bộ Model, View và Controller
trong cùng một project. Thay vào đó, project sử dụng **Layered/Clean
Architecture**, trong đó ASP.NET Core MVC được dùng ở lớp `SocialNetwork.Api`
để tiếp nhận HTTP request.

Luồng xử lý chuẩn:

```text
HTTP Request
    -> Api / Controllers
    -> Application / Services
    -> Application / Interfaces
    -> Infrastructure / Repositories hoặc Services
    -> Database, file system, JWT, realtime
```

Dependency chỉ nên đi theo hướng từ ngoài vào trong:

```text
Api -> Application -> Domain
Infrastructure -> Application + Domain
Domain -> không phụ thuộc project khác
```

## Cấu trúc project

```text
backend/
├── SocialNetwork.sln
├── ARCHITECTURE.md
├── TIEN-DO-BACKEND.md
└── src/
    ├── SocialNetwork.Api/
    │   ├── Controllers/       # HTTP endpoints, routing, status codes
    │   ├── Authentication/    # Current user từ HttpContext
    │   ├── Common/Errors/     # Error response của API
    │   ├── Extensions/        # Đăng ký middleware và service
    │   ├── Hubs/              # SignalR hubs
    │   ├── Middleware/        # Exception handling dùng chung
    │   └── Realtime/          # Adapter SignalR cho application
    │
    ├── SocialNetwork.Application/
    │   ├── Common/             # Exception, storage, realtime contracts
    │   ├── Interfaces/        # Contract dùng chung của application
    │   └── Content/
    │       └── Posts/
    │           ├── Interfaces/ # Service/repository contracts
    │           ├── Requests/   # Input DTO
    │           ├── Responses/  # Output DTO
    │           └── Services/   # Use case và nghiệp vụ ứng dụng
    │
    ├── SocialNetwork.Domain/
    │   ├── Communities/
    │   ├── Content/
    │   ├── Identity/
    │   ├── Messaging/
    │   ├── Moderation/
    │   └── Notification/
    │       # Mỗi module chứa Entities và domain rules
    │
    └── SocialNetwork.Infrastructure/
        ├── Authentication/    # JWT implementation
        ├── Persistence/
        │   ├── Configurations/ # EF Core mappings
        │   ├── Migrations/     # Database migrations
        │   └── Repositories/   # Data access implementation
        ├── Seed/               # Dữ liệu khởi tạo
        └── Storage/            # Local file storage
```

## Quy tắc khi thêm module hoặc feature

1. Đặt entity và quy tắc domain trong `SocialNetwork.Domain/<Module>`.
2. Đặt request, response, service và interface trong
   `SocialNetwork.Application/<Module>/<Feature>`.
3. Đặt repository implementation, EF configuration và integration code trong
   `SocialNetwork.Infrastructure`.
4. Chỉ đặt HTTP concerns như route, authorization và mapping status code trong
   `SocialNetwork.Api/Controllers`.
5. Không gọi `AppDbContext` trực tiếp từ Controller hoặc Application Service.
6. Tái sử dụng các foundation hiện có: `ICurrentUser`, `IFileStorage`,
   `IRealtimeNotifier`, exception dùng chung và `GlobalExceptionHandler`.
7. Đăng ký dependency trong extension phù hợp; không khởi tạo repository hoặc
   service bằng `new` trong Controller.
8. Sau mỗi feature, chạy `dotnet build` và kiểm tra endpoint bằng Swagger hoặc
   file `.http`.

## Đánh giá hiện trạng

- Đã có đầy đủ bốn project chính và dependency direction phù hợp.
- Đã có API Controller, Application Service, repository abstraction và
  Infrastructure implementation cho module Posts.
- Đã tách persistence configuration, migrations, authentication, storage,
  realtime và exception handling.
- Chưa có test project trong solution; đây là hạng mục nên bổ sung khi bắt đầu
  mở rộng nghiệp vụ.
