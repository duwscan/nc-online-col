# Phân công 4 người theo 4 modules và ownership models

Tài liệu này phân công 4 thành viên, mỗi người chịu trách nhiệm chính 1 module chức năng (theo `srs.md` và `srs-enriched.md`) và ownership các model tương ứng (theo `database-design.md`).

## 1) Nguyên tắc ownership

- Mỗi model có đúng 1 owner chính (single owner).
- Module khác được đọc dữ liệu qua contract/service, không sửa trực tiếp logic nghiệp vụ cốt lõi của model không thuộc ownership của mình.
- Thay đổi schema hoặc business rule của model phải được review bởi owner module tương ứng.
- Các model dùng chung vẫn phải có owner rõ ràng để tránh chồng chéo trách nhiệm.

## 2) Bảng phân công 4 người

| Người phụ trách | Module | Phạm vi chức năng chính | Models owner chính |
| --- | --- | --- | --- |
| **Người 1** | **Module 1 - Quản lý tài khoản và phân quyền** | Đăng ký/đăng nhập/quên mật khẩu, phân quyền, audit đăng nhập | `users`, `roles`, `user_roles`, `password_reset_tokens`, `auth_logs` |
| **Người 2** | **Module 2 - Quản lý chương trình đào tạo và đợt tuyển sinh** | Quản lý chương trình/ngành/đợt, cấu hình phương thức xét tuyển, cấu hình giấy tờ theo đợt | `training_programs`, `majors`, `admission_rounds`, `admission_methods`, `round_programs`, `round_admission_methods`, `document_types`, `round_document_requirements` |
| **Người 3** | **Module 3 - Hồ sơ đăng ký tuyển sinh** | Tạo/sửa/nộp hồ sơ, tải tài liệu, quản lý dữ liệu thí sinh và nguyện vọng | `candidates`, `candidate_education_profiles`, `candidate_priority_profiles`, `admission_applications`, `application_preferences`, `application_documents` |
| **Người 4** | **Module 4 - Xét duyệt hồ sơ và thông báo kết quả** | Tiếp nhận và xử lý hồ sơ, cập nhật trạng thái, yêu cầu bổ sung, ghi chú xét duyệt, gửi thông báo | `application_status_histories`, `application_review_notes`, `application_supplement_requests`, `enrollment_confirmations`, `notifications`, `notification_templates`, `audit_logs` |

## 3) Quy định model giao thoa giữa modules

| Model giao thoa | Owner | Module khác được phép làm gì |
| --- | --- | --- |
| `admission_applications` | Người 3 (Module 3) | Module 4 được cập nhật trạng thái thông qua service/transaction chuẩn, không tự thay đổi cấu trúc dữ liệu cốt lõi |
| `application_documents` | Người 3 (Module 3) | Module 4 được đọc để thẩm định; validate nghiệp vụ upload và version file do Module 3 quản lý |
| `application_status_histories` | Người 4 (Module 4) | Module 3 chỉ đọc timeline trạng thái để hiển thị cho thí sinh |
| `notifications` | Người 4 (Module 4) | Module 1/2/3 chỉ phát sinh sự kiện hoặc request gửi thông báo qua API/service |
| `audit_logs` | Người 4 (Module 4) | Module 1/2/3 ghi sự kiện theo contract logging chung, không tự định nghĩa schema riêng |
| `users` | Người 1 (Module 1) | Module 2/3/4 chỉ tham chiếu `user_id`, không quản lý lifecycle tài khoản |

## 4) Trách nhiệm bàn giao và phối hợp

- Mỗi owner chịu trách nhiệm: schema migration, entity/model, repository/DAO, service rule cốt lõi, test nghiệp vụ của models thuộc ownership.
- Mọi thay đổi liên quan public contract giữa modules phải có ít nhất 2 bên review: owner module nguồn + owner module tiêu thụ.
- SLA review nội bộ: owner phản hồi PR thay đổi model của module mình trong tối đa 48 giờ.
- Khi cần tách thêm module báo cáo (Module 5 trong `srs-enriched.md`), ưu tiên tách riêng ownership cho `audit_logs` và nhóm báo cáo để giảm tải Module 4.

## 5) Gợi ý gán người thực tế

Đổi nhãn `Người 1..4` thành tên thật theo đội dự án, ví dụ:

- Người 1 -> [Tên A]
- Người 2 -> [Tên B]
- Người 3 -> [Tên C]
- Người 4 -> [Tên D]
