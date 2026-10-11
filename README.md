# Social Network Backend

Backend cho dự án mạng xã hội, xây dựng bằng **ASP.NET Core (.NET 8)**
theo hướng **Clean Architecture / Modular Monolith**, sử dụng
**PostgreSQL**, **Docker Compose**, **JWT**, **SignalR** và cơ chế lưu
file dùng chung.

> Mục tiêu của README này: thành viên mới clone repo về có thể **cài môi
> trường → chạy database → chạy API → xem database → hiểu các foundation
> đã setup → biết đặt code mới ở đâu và tái sử dụng đúng convention của
> team**.

------------------------------------------------------------------------

## 1. Công nghệ chính

  Công nghệ               Mục đích
  ----------------------- -------------------------------------------
  .NET 8 / ASP.NET Core   Backend API
  Entity Framework Core   ORM, migration, truy cập PostgreSQL
  PostgreSQL 17           Database
  Docker Compose          Khởi động PostgreSQL + Adminer
  Adminer                 Xem và kiểm tra database trên trình duyệt
  Swagger / OpenAPI       Test API
  JWT Bearer              Authentication cho API
  SignalR                 Realtime connection / realtime event
  Local File Storage      Lưu file/ảnh development trong `uploads/`

------------------------------------------------------------------------

## 2. Cài đặt môi trường

Thành viên mới cần cài:

-   **Git**
-   **.NET 8 SDK**
-   **Docker Desktop**
-   IDE tùy chọn: Visual Studio / Rider / VS Code

Kiểm tra sau khi cài:

``` powershell
git --version
dotnet --version
docker --version
docker compose version
```

`dotnet --version` phải là .NET 8.x hoặc môi trường tương thích với
solution.

### NuGet package

Không cần cài từng thư viện NuGet bằng tay nếu package đã được khai báo
trong project.

Sau khi clone, chạy:

``` powershell
dotnet restore
```

Lệnh này tải các package mà solution/project đang sử dụng.

------------------------------------------------------------------------

## 3. Clone và chuẩn bị project

``` powershell
git clone <REPOSITORY_URL>
cd social-network\backend
```

Nếu repo có `.env.example`, tạo `.env`:

``` powershell
Copy-Item .env.example .env
```

`.env` dùng cho môi trường local và **không nên commit secret thật** lên
Git.

------------------------------------------------------------------------

## 4. Biến môi trường

Các nhóm cấu hình development gồm:

``` text
POSTGRES_DB
POSTGRES_USER
POSTGRES_PASSWORD
POSTGRES_PORT

ADMINER_PORT

ConnectionStrings__DefaultConnection
Jwt__Secret
```

### Cấu hình Brevo email API

Backend dùng Brevo API để gửi email xác minh tài khoản và email đặt lại mật khẩu,
không phụ thuộc Gmail App Password. Tạo API key trong Brevo, xác minh sender,
rồi cập nhật các biến trong `backend\.env`:

```powershell
Email__Enabled=true
Email__ApiKey=your-brevo-api-key
Email__FromAddress=your-verified-brevo-sender@example.com
Email__FromName=Social Network
Email__FrontendBaseUrl=http://localhost:3000
```

File `backend\.env` đã được tạo từ `backend\.env.example` để chạy local.
`Email__ApiKey` chỉ được đặt trong biến môi trường local và không được commit
vào repository.

Ngoài ra project có cấu hình development cho JWT issuer/audience, CORS
và Storage trong `appsettings.Development.json`.

### Chạy backend với `.env`

`dotnet run` không tự đọc file `.env`. Script `backend\run-dev.ps1` sẽ tự nạp
các biến vào process rồi chạy API:

```powershell
cd E:\workspace\Project\Net\social-network\backend
.\run-dev.ps1
```

Docker Compose vẫn tự đọc `.env` ở thư mục `social-network`; file đó độc lập
với `.env` của backend.

> Nếu mở terminal mới thì cần nạp lại `.env`.

------------------------------------------------------------------------

## 5. Chạy PostgreSQL và Adminer bằng Docker

Tại thư mục chứa `docker-compose.yml`:

``` powershell
docker compose up -d
```

Kiểm tra container:

``` powershell
docker compose ps
```

Project hiện dùng:

-   PostgreSQL 17
-   Adminer
-   PostgreSQL local port mặc định trong `.env`: `5432`
-   Adminer local port mặc định: `8081`

Mở Adminer:

``` text
http://localhost:8081
```

Thông tin đăng nhập lấy từ `.env`.

Khi Adminer chạy trong Docker và kết nối sang PostgreSQL container,
server thường là:

``` text
postgres
```

Database/user/password dùng các giá trị `POSTGRES_*` trong `.env`.

Dừng container:

``` powershell
docker compose down
```

Muốn xóa luôn volume database local (cẩn thận: mất dữ liệu local):

``` powershell
docker compose down -v
```

------------------------------------------------------------------------

## 6. Restore, build và migration database

Từ backend/solution:

``` powershell
dotnet restore
dotnet build
```

Sau khi PostgreSQL đã chạy và biến môi trường đã được nạp, chạy
migration theo project EF hiện tại của solution.

Nếu đang đứng tại project startup `SocialNetwork.Api` và cấu hình EF của
repo cho phép chạy trực tiếp:

``` powershell
dotnet ef database update
```

Nếu máy chưa có `dotnet-ef`:

``` powershell
dotnet tool install --global dotnet-ef
```

Kiểm tra:

``` powershell
dotnet ef --version
```

> Team đang giữ migration trong source. Không tự tạo lại database schema
> bằng SQL thủ công. Khi schema thay đổi, dùng EF Core migration theo
> convention của project.

### Seed development

Khi API chạy ở môi trường Development, project có `DatabaseSeeder` để
thêm dữ liệu mẫu. Seeder được thiết kế để kiểm tra dữ liệu tồn tại trước
khi thêm nhằm tránh duplicate.

Không nhét dữ liệu riêng của module vào `UserSeeder`. Module cần seed
riêng thì tạo seeder riêng và đăng ký vào `DatabaseSeeder` theo đúng
dependency.

------------------------------------------------------------------------

## 7. Chạy API

Nạp `.env` trước, sau đó chạy project API:

``` powershell
dotnet run
```

Terminal sẽ in URL thực tế, ví dụ:

``` text
https://localhost:<PORT>
http://localhost:<PORT>
```

Mở Swagger bằng URL mà API thực tế in ra:

``` text
https://localhost:<PORT>/swagger
```

Không hard-code port từ README nếu terminal của máy bạn hiển thị port
khác.

------------------------------------------------------------------------

# 8. Cấu trúc source code

Project chia trách nhiệm thành 4 layer chính:

``` text
backend/
│
├── SocialNetwork.Domain
│   └── Entity và domain model
│
├── SocialNetwork.Application
│   ├── Common
│   └── <Module>
│       ├── Interfaces
│       ├── Requests
│       ├── Responses
│       └── Services
│
├── SocialNetwork.Infrastructure
│   ├── Authentication
│   ├── Persistence
│   │   ├── Configurations
│   │   ├── Repositories
│   │   └── Seed
│   └── Storage
│
└── SocialNetwork.Api
    ├── Authentication
    ├── Common
    ├── Controllers
    ├── Extensions
    ├── Hubs
    ├── Middleware
    └── Realtime
```

## Trách nhiệm từng layer

### `SocialNetwork.Domain`

Chứa Entity/domain model.

Ví dụ:

``` text
Domain/
├── Identity/Entities
├── Communities/Entities
├── Content/Entities
├── Messaging/Entities
├── Notification/Entities
└── Moderation/Entities
```

Domain không nên phụ thuộc API, EF Core hay PostgreSQL.

### `SocialNetwork.Application`

Chứa use case/business flow và các contract.

Ví dụ một feature:

``` text
Application/Content/Posts/
├── Interfaces/
│   ├── IPostService.cs
│   └── IPostRepository.cs
├── Requests/
│   └── CreatePostRequest.cs
├── Responses/
│   └── PostResponse.cs
└── Services/
    └── PostService.cs
```

-   `Request`: dữ liệu đầu vào của use case.
-   `Response`: dữ liệu trả ra ngoài.
-   `Service`: xử lý use case/nghiệp vụ.
-   Repository interface: contract mà Application cần để truy cập dữ
    liệu.

### `SocialNetwork.Infrastructure`

Chứa implementation liên quan hạ tầng:

-   EF Core / PostgreSQL
-   Repository implementation
-   JWT implementation
-   File storage
-   Seed

Ví dụ:

``` text
Application:
IPostRepository

        ↓ implemented by

Infrastructure:
PostRepository
```

### `SocialNetwork.Api`

Là entry point HTTP/realtime:

-   Controller
-   Swagger
-   JWT middleware/config
-   Global exception handling
-   SignalR Hub
-   CORS
-   DI wiring

Controller nên mỏng: nhận request → gọi Application service → trả
response.

------------------------------------------------------------------------

# 9. Luồng chuẩn khi viết một API

Luồng mong muốn:

``` text
Client
  ↓
Controller
  ↓
Request / Validation
  ↓
Application Service
  ↓
Repository Interface
  ↓
Infrastructure Repository
  ↓
EF Core
  ↓
PostgreSQL
```

Nếu có lỗi nghiệp vụ:

``` text
Service
  ↓
throw Common Exception
  ↓
GlobalExceptionHandler
  ↓
ApiErrorResponse chuẩn
```

Nếu cần realtime:

``` text
Service
  ↓
DB operation thành công
  ↓
IRealtimeNotifier
  ↓
SignalR
  ↓
Client nhận event
```

------------------------------------------------------------------------

# 10. JWT/Auth foundation - đã setup sẵn

Team **không tự viết lại JWT ở từng module**.

Foundation đã có:

``` text
IJwtTokenGenerator
JwtTokenGenerator
JwtOptions
ICurrentUser
CurrentUser
AuthenticationExtensions
```

JWT access token chứa thông tin user và được ASP.NET JWT Bearer kiểm tra
ở request.

Project đang theo convention:

``` text
API mặc định cần đăng nhập
```

Endpoint public mới thêm:

``` csharp
[AllowAnonymous]
```

## Lấy user hiện tại

Trong Controller/Service phù hợp, dùng abstraction đã setup:

``` csharp
private readonly ICurrentUser _currentUser;

public ExampleController(ICurrentUser currentUser)
{
    _currentUser = currentUser;
}
```

Lấy ID:

``` csharp
var userId = _currentUser.UserId;
```

**Không tự viết lại logic đọc JWT claim trong từng Controller**, ví dụ
không nên rải code `User.FindFirst(...)` khắp module.

Project phát `sub` cho UserId và đã có `ICurrentUser` để thống nhất cách
đọc.

## Access Token và Refresh Token

Foundation hiện cung cấp cơ chế phát/validate **Access Token**.

Module Auth sau này chịu trách nhiệm nghiệp vụ:

``` text
Login
→ kiểm tra User/password
→ tạo Access Token
→ tạo/quản lý Refresh Token
→ refresh/revoke token khi cần
```

Không dùng demo/dev token endpoint làm Login thật.

------------------------------------------------------------------------

# 11. Exception foundation - BẮT BUỘC tái sử dụng

Project đã setup sẵn **một bộ exception chung** để mọi module trả lỗi
thống nhất.

Không nên làm kiểu:

``` csharp
try
{
    ...
}
catch
{
    return BadRequest(...);
}
```

ở từng Controller nếu lỗi đó thuộc luồng nghiệp vụ đã được Global
Exception Handler hỗ trợ.

Các exception dùng chung:

  --------------------------------------------------------------------------------------------
  Exception                                  HTTP Code                        Dùng khi
  ------------------------- --------------------- --------------------------- ----------------
  `ValidationException`                       400 `VALIDATION_ERROR`          Validation
                                                                              nghiệp
                                                                              vụ/request phức
                                                                              tạp

  `UnauthorizedException`                     401 `UNAUTHORIZED`              Chưa/không xác
                                                                              thực

  `ForbiddenException`                        403 `FORBIDDEN`                 Đã đăng nhập
                                                                              nhưng không có
                                                                              quyền

  `NotFoundException`                         404 `NOT_FOUND`                 Không tìm thấy
                                                                              resource

  `ConflictException`                         409 `CONFLICT`                  Dữ liệu/trạng
                                                                              thái bị xung đột

  `BusinessRuleException`                     422 `BUSINESS_RULE_VIOLATION`   Vi phạm business
                                                                              rule

  Exception không xử lý                       500 `INTERNAL_SERVER_ERROR`     Lỗi ngoài dự
                                                                              kiến
  --------------------------------------------------------------------------------------------

Ví dụ resource không tồn tại:

``` csharp
var post = await _postRepository.GetByIdAsync(id, cancellationToken);

if (post is null)
{
    throw new NotFoundException("Post", id);
}
```

Không có quyền:

``` csharp
if (post.AuthorId != currentUserId)
{
    throw new ForbiddenException();
}
```

Conflict:

``` csharp
if (usernameAlreadyExists)
{
    throw new ConflictException("Username already exists.");
}
```

Business rule:

``` csharp
if (!communityAllowsPosting)
{
    throw new BusinessRuleException(
        "This community does not allow posting.");
}
```

`GlobalExceptionHandler` sẽ tự map exception thành response JSON chung,
bao gồm:

``` text
status
code
message
errors (nếu có)
traceId
```

### Quy tắc cho team

> **Có exception dùng chung phù hợp → dùng lại exception đó. Không tự
> tạo format lỗi riêng trong module.**

Chỉ bổ sung exception mới khi thực sự có loại lỗi chung mới mà
foundation chưa biểu diễn được.

------------------------------------------------------------------------

# 12. Validation

Validation format cơ bản có thể đặt trên Request bằng DataAnnotations.

Ví dụ:

``` csharp
public class ExampleRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
```

Các rule phụ thuộc nhiều field, database hoặc business state nên xử lý
trong Application service và dùng exception chung.

Không cần mỗi Controller tự kiểm tra `ModelState` nếu validation
pipeline chung đã xử lý.

------------------------------------------------------------------------

# 13. Upload ảnh/file - dùng foundation có sẵn

Team không tự tạo cơ chế lưu file riêng cho từng module.

Application đã có abstraction dạng:

``` text
IFileStorage
FileUploadRequest
StoredFile
```

Infrastructure có implementation local storage.

Luồng:

``` text
Module
  ↓
IFileStorage.UploadAsync(...)
  ↓
LocalFileStorage
  ↓
uploads/<folder>/...
  ↓
URL /uploads/...
```

Ví dụ các module có thể tái sử dụng:

``` text
User avatar
Post image/video
Community avatar/banner
Message attachment
```

`Program.cs` đã expose thư mục `uploads` thành static files trong
development.

### Quy tắc

-   Module gọi `IFileStorage`, không tự `File.WriteAllBytes` rải rác.
-   DB lưu thông tin media theo Entity của module.
-   File vật lý do storage foundation quản lý.
-   `uploads/` là dữ liệu runtime local, không commit lên Git.

------------------------------------------------------------------------

# 14. Code mẫu Create Post

Create Post là feature mẫu để hiểu flow của một module.

Cấu trúc nên đọc theo thứ tự:

``` text
CreatePostRequest
      ↓
PostsController
      ↓
IPostService
      ↓
PostService
      ↓
IPostRepository
      ↓
PostRepository
      ↓
AppDbContext
      ↓
PostgreSQL
```

Khi làm feature mới, hãy bám flow này thay vì đưa toàn bộ business logic
vào Controller.

------------------------------------------------------------------------

# 15. Realtime foundation - SignalR

Project có một Hub chung:

``` text
/hubs/realtime
```

Hub yêu cầu JWT (`[Authorize]`).

Client kết nối bằng Access Token:

``` text
Client
  ↓ JWT
/hubs/realtime
  ↓
RealtimeHub
  ↓
OnConnectedAsync()
  ↓
user:{UserId}
```

Khi user connect, connection được đưa vào group riêng:

``` text
user:<USER_ID>
```

Một user có thể có nhiều connection (nhiều tab/device) trong cùng group.

## Module gửi realtime như thế nào?

Module **không inject `IHubContext<RealtimeHub>` trực tiếp khắp
source**.

Dùng abstraction chung:

``` text
IRealtimeNotifier
```

Ví dụ:

``` csharp
await _realtimeNotifier.SendToUserAsync(
    receiverId,
    "MessageReceived",
    response,
    cancellationToken);
```

Convention event name: **PascalCase**.

Ví dụ:

``` text
MessageReceived
MessageUpdated
MessageDeleted
NotificationReceived
FriendRequestReceived
CommunityInvitationReceived
```

Database giữ dữ liệu lâu dài. SignalR chỉ dùng để **đẩy thay đổi
realtime**.

Nếu receiver offline, SignalR không thay database. Khi user online lại,
FE phải tải dữ liệu từ API/database.

------------------------------------------------------------------------

# 16. Bài test Realtime Client

Đây là bài test quan trọng để kiểm tra realtime foundation
**end-to-end**, không chỉ kiểm tra Hub có compile hay không.

Nó chứng minh:

``` text
JWT
 ↓
Client tạo SignalR connection
 ↓
/hubs/realtime
 ↓
Server xác định UserId
 ↓
connection vào user:{UserId}
 ↓
API send-to-me
 ↓
IRealtimeNotifier
 ↓
SignalRRealtimeNotifier
 ↓
TestNotification
 ↓
đúng client nhận event
```

## Tại sao tạo test client bên ngoài repo?

Test client chỉ dùng để kiểm tra foundation nên không cần trở thành code
production của SocialNetwork.

Có thể tạo ở thư mục khác, ví dụ:

``` powershell
cd D:\
dotnet new console -n SignalRTestClient
cd SignalRTestClient

dotnet add package Microsoft.AspNetCore.SignalR.Client --version 8.*
```

Thay `Program.cs` của test client:

``` csharp
using Microsoft.AspNetCore.SignalR.Client;

Console.Write("API URL: ");
var apiUrl = Console.ReadLine()!.TrimEnd('/');

Console.Write("Access Token: ");
var accessToken = Console.ReadLine()!.Trim();

var connection = new HubConnectionBuilder()
    .WithUrl($"{apiUrl}/hubs/realtime", options =>
    {
        options.AccessTokenProvider =
            () => Task.FromResult<string?>(accessToken);
    })
    .WithAutomaticReconnect()
    .Build();

connection.On<object>(
    "TestNotification",
    data =>
    {
        Console.WriteLine();
        Console.WriteLine("=== REALTIME EVENT RECEIVED ===");
        Console.WriteLine(data);
        Console.WriteLine("===============================");
    });

connection.Reconnecting += error =>
{
    Console.WriteLine("Reconnecting...");
    return Task.CompletedTask;
};

connection.Reconnected += connectionId =>
{
    Console.WriteLine(
        $"Reconnected. ConnectionId: {connectionId}");
    return Task.CompletedTask;
};

try
{
    Console.WriteLine("Connecting...");

    await connection.StartAsync();

    Console.WriteLine("CONNECTED SUCCESSFULLY");
    Console.WriteLine(
        $"ConnectionId: {connection.ConnectionId}");
    Console.WriteLine("Waiting for TestNotification...");
    Console.WriteLine("Press ENTER to stop.");

    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine("CONNECTION FAILED");
    Console.WriteLine(ex);
}
finally
{
    await connection.DisposeAsync();
}
```

### Đoạn nào thực sự kết nối tới server?

Đây là phần cấu hình connection:

``` csharp
var connection = new HubConnectionBuilder()
    .WithUrl($"{apiUrl}/hubs/realtime", options =>
    {
        options.AccessTokenProvider =
            () => Task.FromResult<string?>(accessToken);
    })
    .WithAutomaticReconnect()
    .Build();
```

Dòng thực sự bắt đầu connection:

``` csharp
await connection.StartAsync();
```

Đoạn client đăng ký nghe event:

``` csharp
connection.On<object>(
    "TestNotification",
    data =>
    {
        Console.WriteLine(data);
    });
```

## Cách chạy bài test

1.  Chạy SocialNetwork API.
2.  Có một Access Token hợp lệ.
3.  Chạy `SignalRTestClient`.
4.  Nhập đúng API URL mà `dotnet run` đang hiển thị.
5.  Nhập Access Token.
6.  Client phải hiện:

``` text
CONNECTED SUCCESSFULLY
ConnectionId: ...
Waiting for TestNotification...
```

7.  Trong Swagger, Authorize bằng **cùng user/token**.
8.  Gọi test endpoint:

``` text
POST /api/realtime-test/send-to-me
```

9.  API trả `200`.
10. Console test client phải nhận:

``` text
=== REALTIME EVENT RECEIVED ===
...
===============================
```

Nếu cả hai xảy ra thì realtime foundation đã chạy end-to-end.

> `RealtimeTestController` và dev token endpoint chỉ là scaffolding để
> kiểm tra foundation. Không xem chúng là API nghiệp vụ production. Có
> thể xóa hoặc chỉ giữ cho Development sau khi team đã test xong.

------------------------------------------------------------------------

# 17. Realtime khi có Frontend React

Test console mô phỏng đúng việc FE sẽ làm sau này.

Sau Login:

``` text
Login thành công
  ↓
FE nhận Access Token
  ↓
FE lưu token theo auth strategy
  ↓
FE start SignalR connection
  ↓
connection tồn tại ở cấp ứng dụng
  ↓
FE đăng ký các event cần nghe
```

Khi logout:

``` text
Logout
  ↓
stop SignalR connection
```

Nên dùng automatic reconnect khi mất mạng.

Ví dụ logic tương đương:

``` javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${apiUrl}/hubs/realtime`, {
        accessTokenFactory: () => accessToken
    })
    .withAutomaticReconnect()
    .build();

await connection.start();

connection.on("MessageReceived", data => {
    // update UI
});
```

> SignalR connection không phải WebRTC media connection. Sau này voice
> room có thể dùng SignalR cho signaling/state realtime, còn audio thực
> tế dùng WebRTC.

------------------------------------------------------------------------

# 18. Khi thêm một feature mới

Ví dụ thành viên nhận task `Create Community`, hãy đi theo flow chung:

1.  Kiểm tra/tạo Entity trong `Domain`.
2.  Tạo Request trong module `Application`.
3.  Tạo Response.
4.  Tạo Service interface.
5.  Implement Service.
6.  Tạo Repository interface nếu cần database.
7.  Implement Repository trong `Infrastructure`.
8.  Đăng ký Dependency Injection.
9.  Tạo Controller trong `Api`.
10. Cần user hiện tại → dùng `ICurrentUser`.
11. Cần upload → dùng `IFileStorage`.
12. Cần realtime → dùng `IRealtimeNotifier`.
13. Có lỗi nghiệp vụ → throw exception foundation phù hợp.
14. Không tự tạo error response format riêng.
15. `dotnet build`.
16. Chạy và test trên Swagger.

Ví dụ cấu trúc:

``` text
SocialNetwork.Domain/
└── Communities/
    └── Entities/
        └── Community.cs

SocialNetwork.Application/
└── Communities/
    └── Communities/
        ├── Interfaces/
        ├── Requests/
        ├── Responses/
        └── Services/

SocialNetwork.Infrastructure/
└── Persistence/
    └── Repositories/
        └── CommunityRepository.cs

SocialNetwork.Api/
└── Controllers/
    └── CommunitiesController.cs
```

Tên folder cụ thể có thể theo module hiện hữu, nhưng **phải giữ cùng
nguyên tắc phân layer**.

------------------------------------------------------------------------

# 19. Những foundation đã có - không tự làm lại

Trước khi tự viết infrastructure mới, kiểm tra project đã có chưa.

  Nhu cầu              Foundation cần dùng
  -------------------- ----------------------------------------------
  Lấy user đăng nhập   `ICurrentUser`
  Tạo access token     `IJwtTokenGenerator`
  Authentication       JWT Bearer foundation
  API lỗi thống nhất   Common Exceptions + `GlobalExceptionHandler`
  Upload file          `IFileStorage`
  Gửi realtime         `IRealtimeNotifier`
  Database             Repository + `AppDbContext`
  Validation HTTP      Validation foundation
  Test API             Swagger

Mục tiêu là tránh:

``` text
Member A làm auth kiểu A
Member B tự đọc JWT kiểu B
Member C tự lưu file kiểu C
Member D tự tạo Hub kiểu D
```

Tất cả module phải tái sử dụng foundation chung.

------------------------------------------------------------------------

# 20. Quick Start

Nếu máy đã cài đủ Git, .NET 8 và Docker:

``` powershell
git clone <REPOSITORY_URL>
cd social-network\backend

Copy-Item .env.example .env

docker compose up -d

dotnet restore
dotnet build
```

Nạp `.env`:

``` powershell
Get-Content .env | ForEach-Object {
    if ($_ -match '^\s*([^#][^=]+)=(.*)$') {
        [Environment]::SetEnvironmentVariable(
            $matches[1].Trim(),
            $matches[2].Trim(),
            "Process"
        )
    }
}
```

Sau đó:

``` powershell
# chạy migration tại project/startup location của solution
dotnet ef database update

dotnet run
```

Cuối cùng:

``` text
Swagger → URL /swagger mà dotnet run hiển thị
Adminer → http://localhost:8081
```

------------------------------------------------------------------------

# 21. Troubleshooting

### `docker compose up` không chạy

Kiểm tra Docker Desktop đã start:

``` powershell
docker info
docker compose ps
```

### Port PostgreSQL/Adminer bị chiếm

Kiểm tra `.env` và đổi port local nếu cần.

Ví dụ:

``` text
POSTGRES_PORT=5433
ADMINER_PORT=8082
```

Nhớ cập nhật connection string tương ứng nếu đổi PostgreSQL port.

### API không đọc được secret/connection string

Đảm bảo đã nạp `.env` trong **cùng terminal** trước `dotnet run`.

Kiểm tra tên environment variable dùng convention ASP.NET:

``` text
Section__Key
```

Ví dụ:

``` text
Jwt__Secret
ConnectionStrings__DefaultConnection
```

### SignalR client báo `actively refused`

Thường là client đang dùng sai API URL/port hoặc API chưa chạy.

Lấy URL trực tiếp từ output:

``` powershell
dotnet run
```

Không mặc định dùng `https://localhost:7001` nếu server không listen
port đó.

### API trả 401 dù có token

Kiểm tra:

-   Token còn hạn.
-   Swagger/client đã gửi Bearer token.
-   Issuer/Audience/Secret khớp.
-   Với code nghiệp vụ, lấy user qua `ICurrentUser` thay vì tự đọc claim
    theo convention khác.

### Lỗi bị Windows/Application Control chặn assembly

Nếu gặp lỗi dạng assembly bị Application Control/Windows security policy
block, kiểm tra chính sách bảo mật của máy và file/project vừa tải về.
Không tắt bảo mật lâu dài chỉ để chạy project; sau khi xác định nguyên
nhân, bật lại protection.

------------------------------------------------------------------------

# 22. Quy ước trước khi push

Trước khi push feature:

``` powershell
dotnet restore
dotnet build
```

Sau đó chạy API và test endpoint liên quan bằng Swagger.

Nếu feature thay đổi schema:

-   tạo migration đúng convention;
-   chạy migration local;
-   kiểm tra database;
-   không sửa database thủ công rồi bỏ quên migration.

Nếu feature dùng foundation chung:

-   không duplicate JWT;
-   không duplicate storage;
-   không duplicate SignalR Hub;
-   không tự format exception;
-   không tự đọc current user theo cách riêng.

------------------------------------------------------------------------

## Tóm tắt

Khi tham gia project, hãy nhớ flow chính:

``` text
Clone
 ↓
.env
 ↓
Docker
 ↓
PostgreSQL + Adminer
 ↓
Restore / Build
 ↓
Migration
 ↓
Run API
 ↓
Swagger
 ↓
Code theo đúng layer
 ↓
Tái sử dụng foundation
```

Và khi code feature:

``` text
Controller
 ↓
Application Service
 ↓
Repository / Storage
 ↓
Database

        + ICurrentUser       nếu cần user đăng nhập
        + Common Exceptions nếu có lỗi
        + IFileStorage       nếu upload
        + IRealtimeNotifier  nếu cần realtime
```

**Không làm lại những gì foundation đã setup sẵn. Giữ cùng cấu trúc và
convention để 4 module có thể phát triển độc lập nhưng vẫn đồng nhất
trong một backend.**
