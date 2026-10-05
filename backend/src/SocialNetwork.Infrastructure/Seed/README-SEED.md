# Seed Foundation

Copy thư mục `Seed` vào:

`SocialNetwork.Infrastructure/Persistence/Seed/`

## 1. Dữ liệu nền hiện có

- Demo User 1: `11111111-1111-1111-1111-111111111111`
- Demo User 2: `22222222-2222-2222-2222-222222222222`
- Demo User 3: `33333333-3333-3333-3333-333333333333`
- Password development: `Demo@123456`

Seeder kiểm tra `Id`, `Email`, `Username` trước khi thêm nên chạy lại không tạo record trùng.

## 2. Package cho PasswordHasher

`UserSeeder` dùng `Microsoft.AspNetCore.Identity.PasswordHasher<User>` để không lưu plain-text password.
Nếu Infrastructure chưa nhận namespace này, thêm package/reference Identity phù hợp với solution trước khi build.

## 3. Gọi seeder khi API khởi động (Development)

Trong `Program.cs`, sau `var app = builder.Build();` và trước `app.Run();`:

```csharp
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Infrastructure.Persistence;
using SocialNetwork.Infrastructure.Persistence.Seed;

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(dbContext);
}
```

Nếu team vẫn muốn migration chạy thủ công, bỏ dòng `MigrateAsync()` và chỉ giữ `DatabaseSeeder.SeedAsync(dbContext)` sau khi đã chạy migration.

## 4. Quy tắc cho thành viên làm module

1. Không sửa `UserSeeder` để nhét data riêng của module.
2. Copy `ExampleModuleSeeder.cs` thành `CommunitySeeder`, `ContentSeeder`, `MessagingSeeder`, ...
3. Nếu data cần tham chiếu User, dùng `SeedIds.DemoUser1Id` (hoặc User 2/3), không hard-code Guid mới rải rác.
4. Data mẫu cần tham chiếu ổn định nên có ID cố định riêng, ví dụ `CommunitySeedIds`.
5. Seeder phải kiểm tra tồn tại trước khi Add để chạy nhiều lần không bị duplicate.
6. Đăng ký seeder mới trong `DatabaseSeeder` theo thứ tự dependency.
7. Seed chỉ phục vụ Development/Test; không dùng demo credentials cho Production.

## 5. Thứ tự dependency mẫu

`UserSeeder -> CommunitySeeder -> ContentSeeder / MessagingSeeder`

Ví dụ Post cần User thì User phải được seed trước. RoomMessage cần Room + User thì Community/Room và User phải tồn tại trước MessagingSeeder.
