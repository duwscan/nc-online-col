# Thực hành tích hợp Ajax – Asp.Net Core MVC
## Module Users - Đỗ Việt Hoàn

### Môn: Lập trình Web Nâng cao
### Thời lượng: 120 phút

---

## 📋 Yêu cầu đã thực hiện

### 1. UI Update: Render bảng dữ liệu bằng JavaScript sử dụng XmlHttpRequest
- **File**: `wwwroot/js/user-ajax.js`
- **Hàm**: `refreshWithXmlHttpRequest()`
- **Mục đích**: Nút "Refresh (XHR)" sử dụng XmlHttpRequest để tải dữ liệu

### 2. CRUD AJAX: Thực hiện Create và Delete bằng jQuery AJAX
- **File**: `wwwroot/js/user-ajax.js`
- **Create**: Hàm trong event listener `btnSave` (chế độ create)
- **Edit**: Hàm trong event listener `btnSave` (chế độ edit)
- **Delete**: Hàm `confirmDelete()`
- **Mục đích**: Tất cả thao tác Create, Edit, Delete đều dùng jQuery AJAX

### 3. Pagination: Dùng Skip/Take trong EF Core và gọi AJAX load trang
- **File**: `Controllers/UserController.cs`
- **Logic**: `Skip((page - 1) * pageSize).Take(pageSize)`
- **Hỗ trợ**: Phân trang với các nút Previous, Next và số trang

### 4. AJAX Search: Controller trả JSON, JS dùng fetch() gọi API
- **File**: `Controllers/UserController.cs` - Action `GetUsers`
- **File**: `wwwroot/js/user-ajax.js`
- **Hàm**: `loadUsers()` - sử dụng Fetch API
- **Mục đích**: Tìm kiếm theo Username, Email, Phone với bộ lọc Status, Role

---

## 🔄 Luồng xử lý Ajax

```
UI → JS → Controller → EF Core → Database → JSON → JS → DOM
```

### Chi tiết từng bước:

#### 1. UI (View - Index.cshtml)
- Người dùng tương tác với bảng dữ liệu, form, nút bấm
- Gọi các hàm JavaScript khi có sự kiện

#### 2. JavaScript (user-ajax.js)
Có 3 cách gọi Ajax:

**a) XmlHttpRequest (XHR)**
```javascript
const xhr = new XMLHttpRequest();
xhr.open('GET', url, true);
xhr.onreadystatechange = function() { ... };
xhr.send();
```
→ Dùng cho nút Refresh

**b) Fetch API**
```javascript
const response = await fetch(url, { method: 'GET' });
const data = await response.json();
```
→ Dùng cho Search và Pagination

**c) jQuery AJAX**
```javascript
$.ajax({
    url: '/User/Create',
    method: 'POST',
    contentType: 'application/json',
    data: JSON.stringify(data),
    success: function(response) { ... }
});
```
→ Dùng cho Create, Edit, Delete

#### 3. Controller (UserController.cs)
- Nhận request từ client
- Xử lý validation
- Gọi EF Core để thao tác database

```csharp
[HttpGet]
public IActionResult GetUsers([FromQuery] UserSearchViewModel search)
{
    var query = _context.Users
        .Include(u => u.UserRoles)
        .Where(u => u.DeletedAt == null)
        .AsQueryable();
    
    // Filter, Skip/Take for pagination
    var users = query
        .Skip((search.Page - 1) * search.PageSize)
        .Take(search.PageSize)
        .ToList();
    
    return Json(result);
}
```

#### 4. Entity Framework Core
- Chuyển đổi LINQ queries thành SQL
- Thực thi truy vấn với Skip/Take cho pagination

#### 5. Database (SQL Server)
- Lưu trữ và truy xuất dữ liệu
- Trả về kết quả cho EF Core

#### 6. JSON Response
- Controller serialize dữ liệu thành JSON
- Gửi về client

```json
{
    "items": [...],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10
}
```

#### 7. JavaScript xử lý response
- Nhận JSON từ server
- Render HTML cho bảng và phân trang

```javascript
function renderUsers(data) {
    let html = '';
    data.items.forEach(function (user) {
        html += `<tr>...</tr>`;
    });
    tbody.innerHTML = html;
}
```

#### 8. DOM Update
- Cập nhật nội dung bảng
- Hiển thị thông tin phân trang
- Hiện thông báo thành công/lỗi

---

## 📁 Cấu trúc files đã tạo

```
wnc/
├── Controllers/
│   └── UserController.cs          # API endpoints cho Ajax
├── Models/
│   └── UserViewModel.cs           # ViewModels cho User
├── Views/
│   └── User/
│       └── Index.cshtml           # Trang User Manager
└── wwwroot/
    └── js/
        └── user-ajax.js           # Xử lý Ajax 3 cách
```

---

## 🚀 Cách chạy và test

### 1. Build và chạy project
```bash
cd wnc
dotnet run
```

### 2. Truy cập User Management
```
https://localhost:xxxx/User
```

### 3. Các chức năng test

| Chức năng | Phương thức Ajax | Mô tả |
|-----------|------------------|-------|
| Load Users | Fetch API | Tự động load khi trang mở |
| Search | Fetch API | Tìm kiếm theo Enter hoặc click |
| Refresh | XmlHttpRequest | Tải lại dữ liệu |
| Create | jQuery AJAX | Tạo user mới |
| Edit | jQuery AJAX | Sửa thông tin user |
| Delete | jQuery AJAX | Xóa user |
| Pagination | Fetch API | Chuyển trang |

---

## 📊 Screenshot luồng

### Giao diện User Manager
- Bảng hiển thị danh sách users
- Form tìm kiếm với bộ lọc
- Nút Create, Refresh
- Phân trang

### Modal Create/Edit
- Form nhập thông tin user
- Checkbox chọn roles
- Validate dữ liệu phía client

---

## 🔑 Key concepts

### XmlHttpRequest
- API cũ nhất của JavaScript cho Ajax
- Hỗ trợ IE6+
- Cần xử lý thủ công readyState

### Fetch API
- API hiện đại, promise-based
- Hỗ trợ async/await
- Cú pháp cleaner

### jQuery AJAX
- Wrapper của XmlHttpRequest
- Hỗ trợ nhiều options
- Global handlers và callbacks

### Skip/Take Pagination
```csharp
Skip((pageNumber - 1) * pageSize).Take(pageSize)
```
- `Skip`: Bỏ qua n phần tử đầu
- `Take`: Lấy n phần tử tiếp theo

---

## 📝 Báo cáo cần nộp

1. **Screenshots** các chức năng Ajax đã thực hiện
2. **Giải thích luồng** UI → JS → Controller → EF Core → DB → JSON → JS → DOM
3. **So sánh** 3 phương pháp Ajax: XmlHttpRequest, Fetch, jQuery AJAX
4. **Code review** các đoạn code quan trọng
