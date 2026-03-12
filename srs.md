SRS – Software Requirements Specification
1. Giới thiệu
1.1. Mục đích

Tài liệu này mô tả yêu cầu phần mềm cho hệ thống Ứng dụng Quản lý Tuyển sinh Hệ đào tạo Trực tuyến. Hệ thống được xây dựng nhằm hỗ trợ nhà trường quản lý toàn bộ quy trình tuyển sinh trực tuyến, từ quản lý tài khoản, quản lý chương trình đào tạo, tiếp nhận hồ sơ đăng ký, cho đến xét duyệt hồ sơ và thông báo kết quả.

1.2. Phạm vi hệ thống

Hệ thống cho phép:

Thí sinh đăng ký tài khoản và nộp hồ sơ trực tuyến

Nhà trường quản lý chương trình đào tạo và các đợt tuyển sinh

Thí sinh theo dõi trạng thái hồ sơ

Cán bộ tuyển sinh xét duyệt hồ sơ

Hệ thống gửi thông báo kết quả đến thí sinh

1.3. Đối tượng sử dụng

Hệ thống có 3 nhóm người dùng chính:

Quản trị viên

Cán bộ tuyển sinh

Thí sinh

1.4. Thuật ngữ

Thí sinh: người đăng ký xét tuyển

Hồ sơ tuyển sinh: tập thông tin và tài liệu do thí sinh nộp

Đợt tuyển sinh: khoảng thời gian mở nhận hồ sơ cho một hoặc nhiều chương trình đào tạo

Xét duyệt hồ sơ: quá trình kiểm tra và đánh giá hồ sơ của thí sinh

2. Mô tả tổng quan hệ thống
2.1. Mục tiêu hệ thống

Hệ thống giúp số hóa quy trình tuyển sinh, giảm thao tác thủ công, tăng khả năng quản lý dữ liệu và nâng cao trải nghiệm cho thí sinh khi đăng ký trực tuyến.

2.2. Chức năng chính

Hệ thống gồm 4 module:

Module 1: Quản lý tài khoản và phân quyền

Module 2: Quản lý chương trình đào tạo và đợt tuyển sinh

Module 3: Hồ sơ đăng ký tuyển sinh

Module 4: Xét duyệt hồ sơ và thông báo kết quả

2.3. Ràng buộc chung

Chỉ người dùng có tài khoản hợp lệ mới được truy cập các chức năng liên quan

Mỗi chức năng hiển thị theo đúng quyền người dùng

Hồ sơ chỉ được nộp trong thời gian mở đợt tuyển sinh

Kết quả xét duyệt chỉ hiển thị khi hồ sơ đã được xử lý

3. Tác nhân của hệ thống
3.1. Quản trị viên

Có quyền quản lý toàn bộ hệ thống, người dùng, phân quyền, chương trình đào tạo, đợt tuyển sinh và theo dõi hoạt động.

3.2. Cán bộ tuyển sinh

Có quyền xem danh sách hồ sơ, kiểm tra hồ sơ, cập nhật trạng thái, duyệt hoặc từ chối hồ sơ, gửi thông báo kết quả.

3.3. Thí sinh

Có quyền đăng ký tài khoản, cập nhật thông tin cá nhân, tạo hồ sơ đăng ký, tải tài liệu, nộp hồ sơ và xem kết quả.

4. Yêu cầu chức năng theo từng module
Module 1. Quản lý tài khoản và phân quyền
4.1. Mục tiêu

Quản lý thông tin người dùng và kiểm soát quyền truy cập theo vai trò.

4.2. Chức năng
4.2.1. Đăng ký tài khoản

Thí sinh có thể tạo tài khoản mới bằng email hoặc số điện thoại, mật khẩu và thông tin cơ bản.

Input:

Họ tên

Email

Số điện thoại

Mật khẩu

Xác nhận mật khẩu

Output:

Tạo tài khoản thành công hoặc báo lỗi

Điều kiện:

Email hoặc số điện thoại chưa tồn tại

Mật khẩu đạt yêu cầu bảo mật

4.2.2. Đăng nhập

Người dùng đăng nhập bằng email hoặc số điện thoại và mật khẩu.

Input:

Tên đăng nhập

Mật khẩu

Output:

Truy cập hệ thống thành công theo quyền

Hoặc thông báo sai thông tin đăng nhập

4.2.3. Quên mật khẩu

Người dùng yêu cầu đặt lại mật khẩu qua email hoặc OTP.

4.2.4. Quản lý hồ sơ tài khoản

Người dùng cập nhật thông tin cá nhân như họ tên, ngày sinh, địa chỉ, ảnh đại diện.

4.2.5. Phân quyền người dùng

Quản trị viên gán vai trò cho tài khoản:

Admin

Cán bộ tuyển sinh

Thí sinh

4.3. Use case chính

UC01: Đăng ký tài khoản

UC02: Đăng nhập

UC03: Quên mật khẩu

UC04: Cập nhật hồ sơ cá nhân

UC05: Gán vai trò người dùng

4.4. Business rules

Một tài khoản chỉ có một email duy nhất

Mỗi người dùng phải có ít nhất một vai trò

Thí sinh không được truy cập chức năng quản trị

Cán bộ tuyển sinh không được gán quyền admin nếu không có phân quyền từ quản trị viên

Module 2. Quản lý chương trình đào tạo và đợt tuyển sinh
4.5. Mục tiêu

Quản lý danh sách ngành học, chương trình đào tạo và đợt tuyển sinh để phục vụ việc nhận hồ sơ.

4.6. Chức năng
4.6.1. Quản lý chương trình đào tạo

Admin tạo, sửa, xóa và xem danh sách chương trình đào tạo.

Thông tin gồm:

Mã chương trình

Tên chương trình

Loại hình đào tạo

Mô tả

Học phí

Thời gian đào tạo

Chỉ tiêu

4.6.2. Quản lý ngành học

Admin tạo và quản lý các ngành học thuộc từng chương trình đào tạo.

4.6.3. Quản lý đợt tuyển sinh

Admin tạo các đợt tuyển sinh với thông tin:

Tên đợt

Ngày bắt đầu

Ngày kết thúc

Chương trình áp dụng

Điều kiện xét tuyển

Trạng thái mở hoặc đóng

4.6.4. Tra cứu thông tin tuyển sinh

Thí sinh xem danh sách chương trình đào tạo và các đợt tuyển sinh đang mở.

4.7. Use case chính

UC06: Tạo chương trình đào tạo

UC07: Cập nhật chương trình đào tạo

UC08: Tạo đợt tuyển sinh

UC09: Cập nhật đợt tuyển sinh

UC10: Xem thông tin tuyển sinh

4.8. Business rules

Mỗi đợt tuyển sinh phải thuộc ít nhất một chương trình đào tạo

Chỉ đợt tuyển sinh đang mở mới nhận hồ sơ

Ngày kết thúc phải lớn hơn ngày bắt đầu

Chỉ tiêu tuyển sinh phải lớn hơn 0

Module 3. Hồ sơ đăng ký tuyển sinh
4.9. Mục tiêu

Cho phép thí sinh tạo, chỉnh sửa, nộp và theo dõi hồ sơ đăng ký tuyển sinh trực tuyến.

4.10. Chức năng
4.10.1. Tạo hồ sơ đăng ký

Thí sinh tạo hồ sơ đăng ký cho một chương trình và một đợt tuyển sinh cụ thể.

Thông tin hồ sơ gồm:

Thông tin cá nhân

Thông tin học vấn

Nguyện vọng

Chương trình đăng ký

Đợt tuyển sinh

Giấy tờ đính kèm

4.10.2. Cập nhật hồ sơ

Thí sinh chỉnh sửa hồ sơ khi hồ sơ chưa nộp hoặc đang được yêu cầu bổ sung.

4.10.3. Tải lên tài liệu

Thí sinh tải tài liệu như:

CCCD hoặc CMND

Học bạ

Bằng tốt nghiệp

Ảnh chân dung

Giấy tờ ưu tiên nếu có

4.10.4. Nộp hồ sơ

Sau khi hoàn tất thông tin, thí sinh gửi hồ sơ vào hệ thống.

4.10.5. Theo dõi trạng thái hồ sơ

Thí sinh xem các trạng thái như:

Nháp

Đã nộp

Đang xét duyệt

Yêu cầu bổ sung

Đã duyệt

Bị từ chối

4.11. Use case chính

UC11: Tạo hồ sơ

UC12: Chỉnh sửa hồ sơ

UC13: Tải tài liệu

UC14: Nộp hồ sơ

UC15: Xem trạng thái hồ sơ

4.12. Business rules

Một hồ sơ chỉ thuộc về một thí sinh

Một hồ sơ chỉ đăng ký cho một chương trình trong một đợt tuyển sinh

Hồ sơ chỉ được nộp khi đủ thông tin bắt buộc

Sau khi nộp, thí sinh không được chỉnh sửa nếu không có yêu cầu bổ sung

Hệ thống phải lưu lịch sử trạng thái hồ sơ

Module 4. Xét duyệt hồ sơ và thông báo kết quả
4.13. Mục tiêu

Hỗ trợ cán bộ tuyển sinh tiếp nhận, kiểm tra, xét duyệt hồ sơ và gửi thông báo kết quả đến thí sinh.

4.14. Chức năng
4.14.1. Xem danh sách hồ sơ

Cán bộ tuyển sinh xem danh sách hồ sơ theo bộ lọc:

Đợt tuyển sinh

Chương trình đào tạo

Trạng thái

Ngày nộp

Thí sinh

4.14.2. Kiểm tra chi tiết hồ sơ

Cán bộ xem toàn bộ thông tin hồ sơ và tài liệu đính kèm.

4.14.3. Cập nhật trạng thái xét duyệt

Cán bộ có thể cập nhật:

Đang xét duyệt

Yêu cầu bổ sung

Đã duyệt

Bị từ chối

4.14.4. Ghi chú xét duyệt

Cán bộ thêm nhận xét nội bộ hoặc lý do từ chối.

4.14.5. Gửi thông báo kết quả

Hệ thống gửi thông báo đến thí sinh qua email hoặc trong hệ thống khi có thay đổi trạng thái hoặc kết quả cuối cùng.

4.15. Use case chính

UC16: Xem danh sách hồ sơ

UC17: Xem chi tiết hồ sơ

UC18: Cập nhật trạng thái hồ sơ

UC19: Ghi chú xét duyệt

UC20: Gửi thông báo kết quả

4.16. Business rules

Chỉ cán bộ tuyển sinh hoặc admin mới được xét duyệt hồ sơ

Mỗi lần thay đổi trạng thái phải lưu người thực hiện và thời gian thực hiện

Khi từ chối hồ sơ phải có lý do

Khi yêu cầu bổ sung phải nêu rõ nội dung cần bổ sung

Mỗi thông báo phải gắn với một hồ sơ cụ thể

5. Yêu cầu phi chức năng
5.1. Hiệu năng

Hệ thống phải phản hồi thao tác thông thường trong vòng dưới 3 giây

Hệ thống phải hỗ trợ nhiều người dùng truy cập đồng thời

5.2. Bảo mật

Mật khẩu phải được mã hóa

Phân quyền phải được kiểm tra ở cả giao diện và backend

Người dùng chỉ được truy cập dữ liệu đúng với quyền của mình

Tài liệu tải lên cần được kiểm tra định dạng và giới hạn dung lượng

5.3. Khả năng sử dụng

Giao diện dễ dùng với thí sinh

Các form phải có kiểm tra dữ liệu đầu vào rõ ràng

Trạng thái hồ sơ phải hiển thị dễ hiểu

5.4. Khả năng mở rộng

Hệ thống có thể mở rộng thêm hình thức tuyển sinh khác

Có thể bổ sung thêm module thanh toán lệ phí xét tuyển trong tương lai

5.5. Độ tin cậy

Dữ liệu hồ sơ phải được lưu chính xác

Hệ thống cần tránh mất dữ liệu khi có lỗi thao tác hoặc lỗi mạng
