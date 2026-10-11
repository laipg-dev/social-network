# Tiến độ phát triển backend Social Network

> Cập nhật: 2026-10-11

## Trạng thái hiện tại

Backend đã được tổ chức theo mô hình **Layered/Clean Architecture** kết hợp
ASP.NET Core MVC ở lớp API. Solution hiện build thành công trên .NET 8.

## Đã hoàn thành

- [x] Tạo solution `SocialNetwork.sln`.
- [x] Tách backend thành bốn project:
  - `SocialNetwork.Api`
  - `SocialNetwork.Application`
  - `SocialNetwork.Domain`
  - `SocialNetwork.Infrastructure`
- [x] Thiết lập dependency direction giữa các layer.
- [x] Tạo nhóm domain entities cho Communities, Content, Identity, Messaging,
      Moderation và Notification.
- [x] Tạo API Controller cho Posts, development authentication và realtime test.
- [x] Tạo application service, request/response DTO và repository contract cho
      Posts.
- [x] Tích hợp Entity Framework Core và PostgreSQL qua Npgsql.
- [x] Tạo `AppDbContext`, EF configurations và migration ban đầu.
- [x] Tạo repository implementation cho Posts.
- [x] Tích hợp JWT authentication và current-user abstraction.
- [x] Tích hợp xử lý lỗi tập trung qua `GlobalExceptionHandler`.
- [x] Tích hợp CORS, Swagger và SignalR realtime.
- [x] Tích hợp local file storage và thư mục upload.
- [x] Tạo database seeder nền tảng.
- [x] Xác nhận `dotnet restore` thành công.
- [x] Xác nhận `dotnet build` thành công: 0 warning, 0 error.
- [x] Xác nhận công cụ `dotnet-ef` đã cài: phiên bản 8.0.6.
- [x] Bổ sung tài liệu kiến trúc tại [ARCHITECTURE.md](./ARCHITECTURE.md).
- [x] Tạo API đăng ký tại `POST /api/auth/register`.
- [x] Tạo API đăng nhập tại `POST /api/auth/login`.
- [x] Hash mật khẩu bằng PBKDF2-SHA256 và so sánh constant-time.
- [x] Tạo `UserProfile` khi đăng ký với display name tùy chọn.
- [x] Chỉ phát hành JWT access token sau khi đăng nhập tài khoản đã xác thực.
- [x] Thêm tài liệu request tại `src/SocialNetwork.Api/Auth.http`.
- [x] Dùng refresh token dạng hash làm auth session, có thời hạn 30 ngày và
      lưu IP/user-agent.
- [x] Tạo endpoint đổi refresh token và xoay vòng token cũ.
- [x] Tạo endpoint logout để revoke refresh token.
- [x] Tạo bảng và luồng email verification bằng mã OTP 6 chữ số, hết hạn sau 15 phút.
- [x] Tạo bảng và luồng password reset có hạn 1 giờ.
- [x] Revoke toàn bộ refresh token khi password được thay đổi.
- [x] Chỉ trả action token trực tiếp trong Development để hỗ trợ môi trường local;
      email provider đã được nối qua Brevo API.
- [x] Tạo và áp dụng migration
      `20261011020608_AddAuthenticationSessionsAndRecovery`.
- [x] Kiểm thử toàn bộ lifecycle: register, verify email, login, refresh,
      logout và reset password.
- [x] Tích hợp Brevo HTTP API để gửi email xác thực và reset password.
- [x] Tạo `IEmailSender` và `BrevoEmailSender` dùng chung cho email xác thực
      và email đặt lại mật khẩu.
- [x] Gửi email xác thực ngay sau đăng ký và khi yêu cầu gửi lại.
- [x] Bắt buộc nhập đúng mã OTP email trước khi tài khoản được phép đăng nhập.
- [x] Đăng ký không trả token; chỉ trả thông báo và email để frontend chuyển sang bước xác nhận.
- [x] Thêm API `GET/PUT /api/profile` để đọc và cập nhật hồ sơ của người dùng hiện tại.
- [x] Bảo vệ profile bằng JWT và kiểm tra trùng username khi cập nhật.
- [x] Thêm `POST /api/profile/avatar` để upload avatar JPG, PNG, WEBP hoặc GIF tối đa 5 MB.
- [x] Trả mã lỗi riêng cho email chưa đăng ký, mật khẩu sai, email chưa xác nhận và tài khoản bị vô hiệu hóa khi đăng nhập.
- [x] Hoàn thiện Posts API: tạo bài viết với community tùy chọn, feed phân trang, xem chi tiết, cập nhật và xóa mềm theo quyền tác giả.
- [x] Bổ sung trường trạng thái bài viết `Status` và migration `20261011143000_AddPostStatus`.
- [x] Hoàn thiện Comments API: tạo comment, reply lồng nhau, lấy danh sách theo bài viết, cập nhật và xóa mềm theo quyền tác giả.
- [x] Bổ sung trường trạng thái comment `Status` và migration `20261011144500_AddCommentStatus`.
- [x] Hoàn thiện Post Reactions API: xem thống kê reaction, đặt/đổi reaction và bỏ reaction.
- [x] Hoàn thiện Comment Reactions API: xem thống kê reaction, đặt/đổi reaction và bỏ reaction.
- [x] Hoàn thiện Post Saves/Bookmarks API: lưu, bỏ lưu, kiểm tra trạng thái và xem danh sách bài viết đã lưu.
- [x] Hoàn thiện Media/PostMedia API: upload media vào bài viết, xem danh sách media và xóa media theo quyền tác giả.
- [x] Hoàn thiện Community API: CRUD community, tìm kiếm, join/leave và tự động thêm owner làm member.
- [x] Bổ sung dữ liệu mẫu cho Home feed: 3 user demo đã xác nhận email và 5 bài viết mẫu.
- [x] Bổ sung profile display name cho user demo; tài khoản demo có thể đăng nhập
      bằng mật khẩu `Demo@123456` trong môi trường Development.
- [x] Hỗ trợ media bài viết với ảnh và video MP4/WEBM tối đa 10 MB mỗi file.
- [x] Gửi email password reset với link frontend có token.
- [x] Không lưu Brevo API key trong source; đọc qua `Email__ApiKey` từ
      environment.
- [x] Thêm cấu hình mẫu tại `.env.example` và hướng dẫn cập nhật Brevo.
- [x] Tạo `backend/.env` cho cấu hình local và `run-dev.ps1` để tự động nạp
      biến môi trường rồi chạy API.

## Đang chờ thực hiện

- [ ] Bổ sung test project cho Domain, Application và API.
- [ ] Hoàn thiện các use case còn lại ngoài Posts.
- [ ] Bổ sung validation chi tiết cho request DTO.
- [ ] Bổ sung authorization policy theo role/permission.
- [ ] Cấu hình Brevo API key và verified sender thực tế trong môi trường local để
      test gửi mail.
- [ ] Tích hợp queue/retry chuyên dụng cho email production.
- [ ] Kiểm tra migration và seed trên PostgreSQL mới.
- [ ] Bổ sung logging, health check và observability cho môi trường triển khai.
- [ ] Cấu hình CI để tự động restore, build và test.

### Kiểm tra gần nhất

- `dotnet build SocialNetwork.sln --no-restore`: thành công, 0 warning, 0 error.
- Database Development: 4 users, 5 posts, 4 user profiles.
- Swagger đã trả HTTP 200 trên `http://localhost:5201/swagger/index.html`.
- Một số lần khởi động sau build bị Windows Application Control chặn DLL
  (`0x800711C7`); đây là giới hạn môi trường local, không phải lỗi compile.

## Quy trình cập nhật tiến độ

Sau mỗi thay đổi backend:

1. Cập nhật mục **Đã hoàn thành** hoặc **Đang chờ thực hiện**.
2. Ghi ngắn gọn các file/project đã thay đổi nếu thay đổi ảnh hưởng kiến trúc.
3. Chạy từ thư mục `backend`:

   ```powershell
   dotnet restore
   dotnet build
   ```

4. Nếu thay đổi database, kiểm tra migration và chạy test trên PostgreSQL
   local trước khi đánh dấu hoàn thành.
