# SRS Mở rộng – Hệ thống Quản lý Tuyển sinh Trực tuyến cho Trường Cao đẳng tại Việt Nam

## 1. Giới thiệu

### 1.1. Mục đích

Tài liệu này mô tả đặc tả yêu cầu phần mềm cho hệ thống Quản lý Tuyển sinh Trực tuyến dành cho một trường cao đẳng tại Việt Nam. Hệ thống được xây dựng nhằm số hóa quy trình tuyển sinh từ khâu tiếp nhận thông tin, công bố đợt tuyển sinh, đăng ký tài khoản, nộp hồ sơ, thẩm định hồ sơ, cập nhật kết quả, gửi thông báo, đến tổng hợp báo cáo và lưu vết nghiệp vụ.

Tài liệu đóng vai trò là cơ sở thống nhất giữa nhà trường, đơn vị phát triển, bộ phận vận hành và bộ phận tuyển sinh về phạm vi, quy tắc nghiệp vụ, yêu cầu chức năng và yêu cầu phi chức năng của hệ thống.

### 1.2. Phạm vi hệ thống

Hệ thống cho phép:

* Thí sinh đăng ký tài khoản và xác thực thông tin cơ bản.
* Thí sinh khai báo thông tin cá nhân, học vấn, nguyện vọng và nộp hồ sơ trực tuyến.
* Nhà trường quản lý chương trình đào tạo, ngành học, đợt tuyển sinh, chỉ tiêu và điều kiện xét tuyển.
* Cán bộ tuyển sinh tiếp nhận, kiểm tra, yêu cầu bổ sung, phê duyệt hoặc từ chối hồ sơ.
* Hệ thống gửi thông báo thay đổi trạng thái và kết quả qua email, thông báo trong hệ thống, và có thể mở rộng sang SMS hoặc Zalo.
* Hệ thống cung cấp báo cáo tổng hợp, thống kê, lịch sử thao tác và nhật ký hệ thống.

Ngoài phạm vi của phiên bản này:

* Quản lý đào tạo sau khi nhập học.
* Quản lý lớp học, thời khóa biểu, điểm học tập.
* Tích hợp đầy đủ với cổng thanh toán, cổng quốc gia hoặc hệ thống ERP nếu nhà trường chưa yêu cầu.

### 1.3. Đối tượng sử dụng

Hệ thống có các nhóm người dùng chính:

* Quản trị viên.
* Cán bộ tuyển sinh.
* Thí sinh.
* Lãnh đạo, phòng ban hỗ trợ hoặc người xem báo cáo.
* Các hệ thống tích hợp bên ngoài như email gateway, SMS gateway, dịch vụ lưu trữ tệp.

### 1.4. Thuật ngữ và định nghĩa

* Thí sinh: Người đăng ký xét tuyển vào trường.
* Hồ sơ tuyển sinh: Tập hợp thông tin và tài liệu mà thí sinh khai báo, tải lên và nộp cho nhà trường.
* Đợt tuyển sinh: Khoảng thời gian mở nhận hồ sơ cho một hoặc nhiều chương trình đào tạo.
* Chương trình đào tạo: Đơn vị tuyển sinh cấp cao đẳng, liên thông, văn bằng 2, từ xa hoặc các hình thức đào tạo khác.
* Nguyện vọng: Lựa chọn đăng ký của thí sinh đối với chương trình đào tạo.
* Phương thức xét tuyển: Cách thức nhà trường sử dụng để đánh giá hồ sơ, ví dụ xét học bạ, xét điểm thi, xét tuyển thẳng hoặc theo tiêu chí nội bộ.
* Điểm ưu tiên: Điểm cộng thêm theo đối tượng, khu vực hoặc quy định của nhà trường và cơ quan quản lý.
* Xác nhận nhập học: Bước thí sinh xác nhận tiếp nhận kết quả trúng tuyển và giữ chỗ theo quy định.
* Xét duyệt hồ sơ: Quá trình kiểm tra tính hợp lệ, đầy đủ và phù hợp của hồ sơ.
* Yêu cầu bổ sung: Trạng thái nhà trường yêu cầu thí sinh cập nhật thêm thông tin hoặc tài liệu.
* Kết quả tuyển sinh: Kết quả cuối cùng sau quá trình xét duyệt hồ sơ.
* Audit log: Bản ghi lưu vết ai đã thao tác, thời điểm nào và thay đổi nội dung gì.

### 1.5. Tài liệu tham chiếu

* Cấu trúc SRS tham khảo theo thông lệ IEEE/ISO về đặc tả yêu cầu phần mềm.
* Mô hình nghiệp vụ tham khảo từ các cổng tuyển sinh và hướng dẫn nộp hồ sơ trực tuyến của các trường đại học, cao đẳng tại Việt Nam.
* Yêu cầu bảo vệ dữ liệu cá nhân tham chiếu theo quy định hiện hành của Việt Nam, bao gồm Nghị định 13/2023/NĐ-CP.
* Các quy chế tuyển sinh, quy định hồ sơ, mốc thời gian và biểu mẫu cụ thể sẽ do nhà trường cấu hình theo từng năm.

## 2. Mô tả tổng quan hệ thống

### 2.1. Mục tiêu hệ thống

Hệ thống hướng đến các mục tiêu sau:

* Số hóa quy trình tuyển sinh và giảm thao tác thủ công.
* Giảm sai sót khi tiếp nhận, đối chiếu và xử lý hồ sơ.
* Tăng tính minh bạch trong trạng thái xử lý, lý do từ chối và lịch sử thay đổi.
* Giúp thí sinh nộp hồ sơ mọi lúc, mọi nơi trên web và thiết bị di động.
* Giúp lãnh đạo và phòng tuyển sinh theo dõi dữ liệu, chỉ tiêu, tỷ lệ nộp hồ sơ và kết quả theo thời gian thực.

### 2.2. Bối cảnh sản phẩm

Hệ thống là một cổng thông tin độc lập hoặc một thành phần trong hệ sinh thái số của nhà trường. Hệ thống có thể kết nối với:

* Hệ thống email của trường để gửi thông báo.
* Hệ thống lưu trữ tệp để lưu giấy tờ đính kèm.
* Hệ thống SMS hoặc Zalo OA để mở rộng kênh thông báo.
* Hệ thống quản lý sinh viên để đồng bộ danh sách trúng tuyển về sau.
* Cổng thanh toán để thu lệ phí xét tuyển trong giai đoạn mở rộng.

### 2.3. Đặc điểm người dùng

#### 2.3.1. Thí sinh

* Sử dụng chủ yếu trên điện thoại và trình duyệt phổ thông.
* Mức độ thành thạo công nghệ không đồng đều.
* Cần giao diện dễ hiểu, các bước ngắn gọn, hướng dẫn rõ ràng, phản hồi tức thì khi nhập liệu sai.

#### 2.3.2. Cán bộ tuyển sinh

* Xử lý khối lượng lớn hồ sơ trong thời gian ngắn.
* Cần bộ lọc, tìm kiếm, xem nhanh, xem chi tiết, nhận xét và thao tác hàng loạt.
* Cần lịch sử thay đổi, thông tin đối chiếu và nhật ký xử lý để phục vụ kiểm tra nội bộ.

#### 2.3.3. Quản trị viên

* Cấu hình chương trình, đợt tuyển sinh, vai trò, phân quyền và tham số hệ thống.
* Theo dõi vận hành, nhật ký lỗi, báo cáo hệ thống và danh sách tài khoản.

#### 2.3.4. Lãnh đạo hoặc người xem báo cáo

* Có nhu cầu xem dashboard, báo cáo tổng hợp, số liệu theo chương trình, đợt, kênh tuyển sinh, địa bàn và kết quả xét duyệt.

### 2.4. Giả định và phụ thuộc

* Nhà trường đã xác định quy trình tuyển sinh nội bộ và tiêu chí xét duyệt cơ bản.
* Nhà trường có hạ tầng email gửi thông báo hoặc sẵn sàng sử dụng nhà cung cấp bên thứ ba.
* Thí sinh có thể tải lên bản scan hoặc ảnh chụp giấy tờ hợp lệ.
* Tiêu chí, chỉ tiêu và đợt tuyển sinh thay đổi theo từng năm sẽ được cấu hình trên hệ thống, không mã hóa cứng trong chương trình.

### 2.5. Ràng buộc chung

* Chỉ người dùng có tài khoản hợp lệ mới được truy cập các chức năng liên quan.
* Mọi chức năng phải hiển thị và được kiểm soát theo đúng quyền người dùng.
* Hồ sơ chỉ được nộp trong thời gian đợt tuyển sinh đang mở và còn hiệu lực.
* Kết quả xét duyệt chỉ hiển thị khi hồ sơ đã được xử lý.
* Mọi thay đổi quan trọng liên quan đến hồ sơ phải được lưu vết.
* Dữ liệu nhạy cảm của thí sinh phải được bảo vệ trong quá trình lưu trữ và truyền tải.

## 3. Tác nhân và vai trò hệ thống

### 3.1. Quản trị viên

Có quyền quản lý toàn bộ hệ thống, quản lý tài khoản, phân quyền, danh mục, chương trình đào tạo, ngành học, đợt tuyển sinh, tham số cấu hình, xem báo cáo tổng hợp và theo dõi nhật ký hệ thống.

### 3.2. Cán bộ tuyển sinh

Có quyền xem danh sách hồ sơ, lọc và tìm kiếm hồ sơ, kiểm tra nội dung, đối chiếu tài liệu, cập nhật trạng thái, ghi chú nghiệp vụ, yêu cầu bổ sung, phê duyệt hoặc từ chối hồ sơ, gửi thông báo nghiệp vụ cho thí sinh.

### 3.3. Thí sinh

Có quyền đăng ký tài khoản, đăng nhập, cập nhật hồ sơ cá nhân, tạo hồ sơ đăng ký, tải tài liệu, nộp hồ sơ, bổ sung thông tin khi được yêu cầu, theo dõi trạng thái và nhận thông báo.

### 3.4. Hệ thống email/SMS/Zalo

Là tác nhân bên ngoài nhận yêu cầu gửi thông báo từ hệ thống và trả về kết quả gửi thành công hoặc thất bại.

### 3.5. Hệ thống lưu trữ tệp

Lưu trữ tài liệu đính kèm và cung cấp liên kết, metadata, trạng thái tệp hoặc lỗi tải lên.

## 4. Mô hình nghiệp vụ tổng quát

### 4.1. Luồng nghiệp vụ chính

1. Nhà trường khai báo chương trình đào tạo, ngành học, đợt tuyển sinh, chỉ tiêu và điều kiện xét tuyển.
2. Thí sinh tạo tài khoản, xác thực thông tin và đăng nhập.
3. Thí sinh tạo hồ sơ, khai báo thông tin, chọn chương trình, tải giấy tờ và lưu nháp.
4. Thí sinh nộp hồ sơ trong thời gian đợt còn mở.
5. Cán bộ tuyển sinh kiểm tra hồ sơ, có thể yêu cầu bổ sung nếu thiếu hoặc sai.
6. Thí sinh bổ sung thông tin theo yêu cầu.
7. Cán bộ cập nhật kết quả cuối cùng: đã duyệt hoặc bị từ chối.
8. Hệ thống gửi thông báo kết quả và lưu lịch sử xử lý.
9. Lãnh đạo và quản trị viên xem báo cáo tổng hợp, thống kê và audit log.

### 4.2. Trạng thái hồ sơ

* Nháp.
* Chờ nộp.
* Đã nộp.
* Đang kiểm tra.
* Yêu cầu bổ sung.
* Đã bổ sung.
* Đang xét duyệt.
* Đã duyệt.
* Bị từ chối.
* Hủy bởi thí sinh hoặc hết hạn.

### 4.3. Quy tắc chuyển trạng thái

* Hồ sơ ở trạng thái Nháp hoặc Chờ nộp mới được phép chỉnh sửa tự do.
* Sau khi nộp, hồ sơ chuyển sang Đã nộp và chưa cho chỉnh sửa trực tiếp.
* Cán bộ có thể chuyển hồ sơ sang Đang kiểm tra, Yêu cầu bổ sung, Đang xét duyệt, Đã duyệt hoặc Bị từ chối theo quyền.
* Nếu hồ sơ ở trạng thái Yêu cầu bổ sung, thí sinh được phép cập nhật các trường và tài liệu trong phạm vi được yêu cầu.
* Mỗi lần thay đổi trạng thái phải lưu người thực hiện, thời gian, lý do và ghi chú liên quan.

## 5. Yêu cầu chức năng

### 5.1. Module 1 - Quản lý tài khoản và phân quyền

#### 5.1.1. Mục tiêu

Quản lý thông tin người dùng và kiểm soát quyền truy cập theo vai trò.

#### 5.1.2. Chức năng

##### FR-01. Đăng ký tài khoản

* Thí sinh tạo tài khoản bằng email hoặc số điện thoại.
* Hệ thống yêu cầu họ tên, email, số điện thoại, mật khẩu và xác nhận mật khẩu.
* Hệ thống kiểm tra email hoặc số điện thoại chưa tồn tại.
* Hệ thống thông báo thành công hoặc lỗi rõ ràng.

##### FR-02. Đăng nhập

* Người dùng đăng nhập bằng email hoặc số điện thoại và mật khẩu.
* Hệ thống xác thực và điều hướng đến giao diện đúng quyền.
* Hệ thống ghi nhật ký đăng nhập thành công và thất bại.

##### FR-03. Quên mật khẩu

* Người dùng yêu cầu đặt lại mật khẩu qua email hoặc OTP.
* Hệ thống sinh mã xác nhận có thời hạn và cho phép tạo mật khẩu mới.

##### FR-04. Quản lý hồ sơ tài khoản

* Người dùng cập nhật thông tin cá nhân như họ tên, ngày sinh, giới tính, địa chỉ, tỉnh/thành, ảnh đại diện.
* Hệ thống lưu lịch sử cập nhật đối với các trường nhạy cảm khi cần.

##### FR-05. Phân quyền người dùng

* Quản trị viên gán vai trò cho tài khoản.
* Vai trò mặc định của tài khoản tự đăng ký là Thí sinh.
* Hệ thống hỗ trợ các vai trò: Quản trị viên, Cán bộ tuyển sinh, Người xem báo cáo, Thí sinh.

#### 5.1.3. Use case chính

* UC01: Đăng ký tài khoản.
* UC02: Đăng nhập.
* UC03: Quên mật khẩu.
* UC04: Cập nhật hồ sơ cá nhân.
* UC05: Gán vai trò người dùng.

#### 5.1.4. Business rules

* Một tài khoản chỉ có một email duy nhất.
* Số điện thoại, nếu được sử dụng để đăng nhập, phải là duy nhất trên hệ thống.
* Mật khẩu phải đáp ứng chính sách bảo mật của nhà trường.
* Thí sinh không được truy cập chức năng quản trị.
* Cán bộ tuyển sinh không được tự gán quyền quản trị.

### 5.2. Module 2 - Quản lý chương trình đào tạo và đợt tuyển sinh

#### 5.2.1. Mục tiêu

Quản lý danh mục ngành học, chương trình đào tạo, đợt tuyển sinh, chỉ tiêu và điều kiện nhận hồ sơ.

#### 5.2.2. Chức năng

##### FR-06. Quản lý chương trình đào tạo

* Quản trị viên tạo, sửa, ngừng hiển thị và xem danh sách chương trình.
* Thông tin gồm: mã chương trình, tên chương trình, loại hình đào tạo, mô tả, học phí dự kiến, thời gian đào tạo, chỉ tiêu, đơn vị quản lý và trạng thái.

##### FR-07. Quản lý ngành học

* Quản trị viên tạo và quản lý các ngành học thuộc từng chương trình đào tạo.
* Hệ thống hỗ trợ gắn mã ngành, tên ngành, mô tả, chỉ tiêu và ưu tiên hiển thị.

##### FR-08. Quản lý đợt tuyển sinh

* Quản trị viên tạo đợt tuyển sinh với tên đợt, năm tuyển sinh, ngày bắt đầu, ngày kết thúc, chương trình áp dụng, điều kiện xét tuyển, trạng thái, ghi chú và cấu hình giấy tờ bắt buộc.
* Quản trị viên có thể cấu hình thêm phương thức xét tuyển, tổ hợp xét tuyển, ngưỡng đầu vào, quy tắc tính điểm và chính sách điểm ưu tiên nếu nhà trường áp dụng.
* Hệ thống cho phép mở, đóng, tạm ngừng và sao chép cấu hình đợt từ năm trước.

##### FR-09. Công bố thông tin tuyển sinh

* Thí sinh xem danh sách chương trình và các đợt đang mở.
* Hệ thống hiển thị thông tin về điều kiện, học phí dự kiến, chỉ tiêu, hạn nộp, tài liệu bắt buộc và hướng dẫn nộp hồ sơ.

##### FR-10. Cấu hình yêu cầu hồ sơ theo đợt

* Quản trị viên cấu hình danh mục giấy tờ bắt buộc và tùy chọn cho từng đợt hoặc từng chương trình.
* Hệ thống cho phép đánh dấu giấy tờ có cần công chứng, có cần bản gốc khi nhập học hay chỉ cần bản scan.

#### 5.2.3. Use case chính

* UC06: Tạo chương trình đào tạo.
* UC07: Cập nhật chương trình đào tạo.
* UC08: Tạo đợt tuyển sinh.
* UC09: Cập nhật đợt tuyển sinh.
* UC10: Xem thông tin tuyển sinh.

#### 5.2.4. Business rules

* Mỗi đợt tuyển sinh phải thuộc ít nhất một chương trình đào tạo.
* Chỉ đợt đang mở mới nhận hồ sơ.
* Ngày kết thúc phải lớn hơn ngày bắt đầu.
* Chỉ tiêu tuyển sinh phải lớn hơn 0.
* Điều kiện xét tuyển và danh mục hồ sơ bắt buộc phải được chốt trước khi mở đợt.

### 5.3. Module 3 - Hồ sơ đăng ký tuyển sinh

#### 5.3.1. Mục tiêu

Cho phép thí sinh tạo, chỉnh sửa, nộp và theo dõi hồ sơ đăng ký tuyển sinh trực tuyến.

#### 5.3.2. Chức năng

##### FR-11. Tạo hồ sơ đăng ký

* Thí sinh tạo hồ sơ cho một chương trình và một đợt tuyển sinh cụ thể.
* Hồ sơ gồm: thông tin cá nhân, thông tin liên hệ, thông tin học vấn, thông tin ưu tiên, nguyện vọng, chương trình đăng ký, đợt tuyển sinh và giấy tờ đính kèm.

##### FR-12. Lưu nháp và cập nhật hồ sơ

* Thí sinh có thể lưu nháp và tiếp tục hoàn thiện hồ sơ sau.
* Thí sinh được chỉnh sửa hồ sơ khi chưa nộp hoặc đang ở trạng thái Yêu cầu bổ sung.

##### FR-13. Tải lên tài liệu

* Thí sinh tải lên các tài liệu như CCCD/CMND, học bạ, bằng tốt nghiệp hoặc giấy chứng nhận tốt nghiệp tạm thời, ảnh chân dung, giấy tờ ưu tiên, giấy khai sinh và hồ sơ khác theo cấu hình đợt.
* Hệ thống kiểm tra định dạng, kích thước, tên tệp, số lượng tệp và trạng thái tải lên.
* Hệ thống lưu metadata của tệp, không chỉ đường dẫn tệp tạm.

##### FR-14. Nộp hồ sơ

* Thí sinh nộp hồ sơ sau khi hoàn tất thông tin và tài liệu bắt buộc.
* Hệ thống xác nhận lần cuối trước khi nộp.
* Hệ thống gán mã hồ sơ duy nhất, đóng dấu thời gian nộp và chuyển trạng thái sang Đã nộp.

##### FR-15. Theo dõi trạng thái hồ sơ

* Thí sinh xem lịch sử trạng thái và ghi chú được phép hiển thị.
* Hệ thống hiển thị các trạng thái như Nháp, Đã nộp, Đang kiểm tra, Yêu cầu bổ sung, Đang xét duyệt, Đã duyệt, Bị từ chối.

##### FR-16. Bổ sung hồ sơ theo yêu cầu

* Khi cán bộ yêu cầu bổ sung, thí sinh nhận được nội dung cần bổ sung.
* Thí sinh có thể cập nhật đúng các mục được yêu cầu và nộp lại trong thời hạn.

#### 5.3.3. Use case chính

* UC11: Tạo hồ sơ.
* UC12: Chỉnh sửa hồ sơ.
* UC13: Tải tài liệu.
* UC14: Nộp hồ sơ.
* UC15: Xem trạng thái hồ sơ.
* UC16: Bổ sung hồ sơ.

#### 5.3.4. Business rules

* Mỗi hồ sơ chỉ thuộc về một thí sinh.
* Mỗi hồ sơ chỉ đăng ký cho một chương trình trong một đợt tuyển sinh.
* Hồ sơ chỉ được nộp khi đủ thông tin bắt buộc.
* Sau khi nộp, thí sinh không được chỉnh sửa nếu không có yêu cầu bổ sung.
* Hệ thống phải lưu lịch sử trạng thái hồ sơ.
* Hệ thống phải ngăn nộp trùng lặp vượt quá quy định do nhà trường thiết lập.

### 5.4. Module 4 - Xét duyệt hồ sơ và thông báo kết quả

#### 5.4.1. Mục tiêu

Hỗ trợ cán bộ tuyển sinh tiếp nhận, kiểm tra, xét duyệt hồ sơ và thông báo kết quả đến thí sinh.

#### 5.4.2. Chức năng

##### FR-17. Xem danh sách hồ sơ

* Cán bộ xem danh sách hồ sơ theo bộ lọc: đợt tuyển sinh, chương trình đào tạo, ngành, trạng thái, ngày nộp, tỉnh/thành, đối tượng ưu tiên và thí sinh.
* Hệ thống hỗ trợ sắp xếp, tìm kiếm, phân trang và xuất danh sách.

##### FR-18. Kiểm tra chi tiết hồ sơ

* Cán bộ xem toàn bộ thông tin hồ sơ và tài liệu đính kèm.
* Hệ thống hiển thị lịch sử xử lý, ghi chú và các lần bổ sung trước đó.

##### FR-19. Cập nhật trạng thái xét duyệt

* Cán bộ cập nhật các trạng thái Đang kiểm tra, Yêu cầu bổ sung, Đang xét duyệt, Đã duyệt, Bị từ chối.
* Hệ thống yêu cầu ghi lý do đối với trạng thái từ chối và nội dung cụ thể đối với trạng thái Yêu cầu bổ sung.

##### FR-20. Ghi chú xét duyệt

* Cán bộ thêm ghi chú nội bộ hoặc ghi chú hiển thị cho thí sinh.
* Hệ thống phân biệt mức độ hiển thị của ghi chú.

##### FR-21. Gửi thông báo kết quả

* Hệ thống gửi thông báo khi thay đổi trạng thái, khi có yêu cầu bổ sung và khi có kết quả cuối cùng.
* Kênh gửi mặc định là email và thông báo trong hệ thống; các kênh khác là tùy chọn mở rộng.

##### FR-22. Thao tác hàng loạt

* Cán bộ được phép gán nhãn, xuất danh sách, gửi thông báo hoặc cập nhật một số thuộc tính cho nhiều hồ sơ cùng lúc theo quyền được cấp.

##### FR-23. Xác nhận nhập học và danh sách trúng tuyển tạm thời

* Hệ thống có khả năng mở rộng để quản lý danh sách đủ điều kiện trúng tuyển, gửi hướng dẫn xác nhận nhập học và ghi nhận trạng thái xác nhận nhập học trực tuyến nếu nhà trường yêu cầu.
* Chức năng này có thể được bật hoặc tắt theo cấu hình của từng đợt tuyển sinh.

#### 5.4.3. Use case chính

* UC17: Xem danh sách hồ sơ.
* UC18: Xem chi tiết hồ sơ.
* UC19: Cập nhật trạng thái hồ sơ.
* UC20: Ghi chú xét duyệt.
* UC21: Gửi thông báo kết quả.
* UC22: Xử lý hàng loạt.
* UC23: Xác nhận nhập học.

#### 5.4.4. Business rules

* Chỉ cán bộ tuyển sinh hoặc quản trị viên mới được xét duyệt hồ sơ.
* Mỗi lần thay đổi trạng thái phải lưu người thực hiện và thời gian thực hiện.
* Khi từ chối hồ sơ phải có lý do.
* Khi yêu cầu bổ sung phải nêu rõ nội dung cần bổ sung và hạn bổ sung nếu có.
* Mỗi thông báo phải gắn với một hồ sơ cụ thể.

### 5.5. Module 5 - Báo cáo, thống kê và audit

#### 5.5.1. Mục tiêu

Hỗ trợ nhà trường theo dõi hiệu quả tuyển sinh, số liệu vận hành và kiểm soát tính minh bạch trong nghiệp vụ.

#### 5.5.2. Chức năng

##### FR-24. Dashboard tổng hợp

* Hiển thị tổng số hồ sơ theo đợt, theo chương trình, theo trạng thái, theo ngày và theo kênh.

##### FR-25. Báo cáo chỉ tiêu và tỷ lệ chuyển đổi

* Báo cáo số thí sinh đăng ký, số hồ sơ đã nộp, số hồ sơ đủ điều kiện, số hồ sơ đã duyệt và tỷ lệ chuyển đổi qua từng giai đoạn.
* Nếu nhà trường áp dụng nhiều nguyện vọng hoặc nhiều phương thức, báo cáo cần phân tách theo phương thức xét tuyển, tổ hợp và nguồn dữ liệu.

##### FR-26. Báo cáo nghiệp vụ

* Báo cáo hồ sơ thiếu giấy tờ, hồ sơ quá hạn bổ sung, hồ sơ đang chờ xử lý và cán bộ đang phụ trách.

##### FR-27. Audit log và lịch sử thao tác

* Hệ thống lưu vết đăng nhập, cập nhật dữ liệu, thay đổi trạng thái, thay đổi phân quyền, xóa mềm và thao tác xuất dữ liệu.
* Quản trị viên có thể tra cứu audit log theo thời gian, người dùng, đối tượng và hành động.

#### 5.5.3. Business rules

* Dữ liệu báo cáo phải phù hợp với dữ liệu gốc tại thời điểm truy vấn.
* Audit log không được phép sửa thủ công bởi người dùng thông thường.

## 6. Yêu cầu giao diện và tích hợp

### 6.1. Giao diện người dùng

* Giao diện web phải hỗ trợ tốt trên desktop và mobile.
* Form phải có kiểm tra dữ liệu đầu vào rõ ràng, thông báo lỗi dễ hiểu và hướng dẫn cho các trường khó hiểu.
* Trạng thái hồ sơ phải được hiển thị dễ hiểu, có màu sắc hoặc nhãn trạng thái nhất quán.
* Các trang dành cho thí sinh phải ưu tiên luồng thao tác ngắn gọn, dễ theo dõi tiến độ hồ sơ.

### 6.2. Giao diện phần mềm bên ngoài

* Tích hợp email để gửi thông báo tài khoản, đặt lại mật khẩu, trạng thái hồ sơ và kết quả tuyển sinh.
* Tích hợp dịch vụ lưu trữ tệp để lưu giấy tờ đính kèm.
* Có khả năng tích hợp SMS/Zalo để gửi thông báo ngắn.
* Có khả năng tích hợp cổng thanh toán để thu lệ phí xét tuyển trong giai đoạn sau.
* Có khả năng tích hợp hệ thống quản lý sinh viên để chuyển dữ liệu trúng tuyển.
* Có khả năng tích hợp với nguồn dữ liệu xét tuyển bên ngoài hoặc file import do nhà trường quy định, ví dụ điểm thi, danh sách ưu tiên hoặc danh sách trúng tuyển.

### 6.3. Giao diện truyền thông

* Mọi kết nối bên ngoài phải đi qua giao thức an toàn như HTTPS.
* Hệ thống phải có cơ chế timeout, retry hợp lý và ghi log khi giao tiếp thất bại với dịch vụ bên ngoài.

## 7. Yêu cầu dữ liệu

### 7.1. Thực thể dữ liệu chính

* Người dùng.
* Vai trò.
* Thí sinh.
* Hồ sơ tuyển sinh.
* Chương trình đào tạo.
* Ngành học.
* Đợt tuyển sinh.
* Tệp đính kèm.
* Lịch sử trạng thái.
* Thông báo.
* Audit log.

### 7.2. Thuộc tính dữ liệu quan trọng

* Thí sinh: họ tên, ngày sinh, giới tính, CCCD/CMND, email, số điện thoại, địa chỉ, tỉnh/thành.
* Hồ sơ: mã hồ sơ, mã thí sinh, mã chương trình, mã đợt, trạng thái, ngày tạo, ngày nộp, ngày cập nhật cuối, ghi chú.
* Cấu hình xét tuyển: phương thức xét tuyển, tổ hợp, điểm sàn, điểm ưu tiên, ngưỡng đạt, chỉ tiêu.
* Tệp đính kèm: loại tệp, tên tệp, dung lượng, định dạng, thời gian tải lên, người tải lên, trạng thái hợp lệ.
* Lịch sử trạng thái: trạng thái cũ, trạng thái mới, người thao tác, thời điểm, lý do.

### 7.3. Ràng buộc toàn vẹn dữ liệu

* Email và số điện thoại đăng nhập phải là duy nhất theo quy tắc cấu hình.
* Mã hồ sơ phải là duy nhất trên toàn hệ thống.
* Hồ sơ không được tham chiếu đến chương trình hoặc đợt đã bị vô hiệu hóa nếu không có cơ chế chuyển đổi dữ liệu.
* Tệp đính kèm phải gắn với một hồ sơ hoặc một tài khoản cụ thể.

### 7.4. Lưu trữ và vòng đời dữ liệu

* Hệ thống phải có chính sách lưu trữ và sao lưu dữ liệu định kỳ.
* Dữ liệu hồ sơ và audit log cần được lưu trong thời hạn do nhà trường quy định.
* Các thao tác xóa dữ liệu nhạy cảm phải theo quy trình được phê duyệt và có lưu vết.

## 8. Yêu cầu phi chức năng

### 8.1. Hiệu năng

* Hệ thống phải phản hồi thao tác thông thường trong vòng dưới 3 giây trong điều kiện tải thông thường.
* Hệ thống phải hỗ trợ nhiều người dùng truy cập đồng thời trong các giai đoạn cao điểm tuyển sinh.
* Các trang danh sách lớn phải hỗ trợ phân trang hoặc tải dữ liệu theo từng phần.

### 8.2. Sẵn sàng và độ tin cậy

* Hệ thống phải hạn chế mất dữ liệu khi có lỗi thao tác, lỗi mạng hoặc mất kết nối tạm thời.
* Hệ thống phải có cơ chế backup và phục hồi dữ liệu.
* Hệ thống cần có khả năng tiếp tục vận hành khi một dịch vụ thông báo bên ngoài tạm thời gặp lỗi.

### 8.3. Bảo mật

* Mật khẩu phải được mã hóa.
* Phân quyền phải được kiểm tra ở cả giao diện và backend.
* Người dùng chỉ được truy cập dữ liệu đúng với quyền của mình.
* Tài liệu tải lên phải được kiểm tra định dạng, giới hạn dung lượng và quét nguy cơ cơ bản.
* Dữ liệu nhạy cảm như CCCD/CMND, email, số điện thoại phải được bảo vệ khi lưu trữ và truyền tải.
* Hệ thống phải hỗ trợ khóa tạm thời hoặc giới hạn số lần đăng nhập sai theo cấu hình.

### 8.4. Khả năng sử dụng

* Giao diện phải dễ sử dụng đối với thí sinh.
* Các form phải có kiểm tra dữ liệu đầu vào rõ ràng.
* Thông báo lỗi phải cụ thể, dễ hiểu và chỉ rõ mục cần sửa.
* Hồ sơ phải hiển thị tiến độ hoàn thành để giúp thí sinh biết còn thiếu mục nào.

### 8.5. Khả năng mở rộng

* Hệ thống có thể mở rộng thêm các hình thức tuyển sinh khác.
* Có thể bổ sung module thanh toán lệ phí xét tuyển trong tương lai.
* Có thể mở rộng thêm vai trò và báo cáo mà không ảnh hưởng lớn đến nghiệp vụ hiện tại.

### 8.6. Khả năng bảo trì

* Danh mục, vai trò, cấu hình thông báo, cấu hình giấy tờ bắt buộc và đợt tuyển sinh phải được quản lý qua giao diện quản trị.
* Hệ thống cần có nhật ký lỗi và monitoring cơ bản để hỗ trợ vận hành.

### 8.7. Tính tuân thủ

* Hệ thống phải hỗ trợ nhà trường thực hiện nghĩa vụ bảo vệ dữ liệu cá nhân theo quy định hiện hành.
* Việc thu thập, lưu trữ, sử dụng và chia sẻ dữ liệu thí sinh phải có mục đích rõ ràng, đúng phạm vi và có lưu vết khi cần.

## 9. Báo cáo và chỉ số nghiệp vụ

### 9.1. Báo cáo cho phòng tuyển sinh

* Số hồ sơ mới theo ngày.
* Số hồ sơ đã nộp theo đợt.
* Số hồ sơ đang chờ bổ sung.
* Số hồ sơ được duyệt, bị từ chối, quá hạn xử lý.
* Danh sách hồ sơ thiếu tài liệu theo loại.
* Danh sách thí sinh đã xác nhận nhập học hoặc chưa xác nhận, nếu nhà trường sử dụng chức năng này.

### 9.2. Báo cáo cho lãnh đạo

* Tỷ lệ nộp hồ sơ theo chương trình.
* Mức độ đạt chỉ tiêu theo đợt và theo ngành.
* Phân bố hồ sơ theo địa bàn, đối tượng ưu tiên hoặc kênh tiếp cận.
* Tỷ lệ chuyển đổi từ tạo tài khoản đến nộp hồ sơ và từ nộp hồ sơ đến trúng tuyển.
* Nếu có áp dụng nhiều phương thức, báo cáo số lượng và kết quả theo từng phương thức xét tuyển.

## 10. Ràng buộc nghiệp vụ tổng hợp

* Mỗi thí sinh chỉ được sử dụng danh tính hợp lệ theo quy định của nhà trường.
* Mỗi hồ sơ phải gắn với một đợt và một chương trình hợp lệ.
* Hồ sơ hết hạn bổ sung có thể bị khóa xử lý tiếp theo quy định của nhà trường.
* Các tiêu chí xét duyệt có thể khác nhau theo chương trình, nhưng phải được cấu hình rõ ràng trước khi áp dụng.
* Nếu nhà trường áp dụng mô hình nhiều nguyện vọng, thứ tự ưu tiên và quy tắc xét trúng tuyển phải được công bố rõ ràng trong cấu hình đợt.
* Các thông báo chính thức đến thí sinh phải có mẫu nội dung được phê duyệt.

## 11. Yêu cầu xác minh và chấp nhận

### 11.1. Tiêu chí chấp nhận chức năng

* Thí sinh có thể hoàn thành chu trình đăng ký tài khoản, tạo hồ sơ, tải tệp và nộp hồ sơ mà không cần can thiệp thủ công.
* Cán bộ có thể xem, lọc, kiểm tra, cập nhật trạng thái và gửi thông báo cho hồ sơ.
* Quản trị viên có thể cấu hình chương trình, đợt tuyển sinh, vai trò và xem audit log.
* Báo cáo có thể tra cứu theo đợt, chương trình và trạng thái.

### 11.2. Tiêu chí chấp nhận phi chức năng

* Hiệu năng thao tác thông thường đạt mức quy định.
* Các chức năng phân quyền, mã hóa mật khẩu và kiểm soát tệp tải lên hoạt động đúng.
* Audit log được tạo khi thay đổi trạng thái và thay đổi phân quyền.

## 12. Đề xuất mở rộng trong tương lai

* Thanh toán lệ phí xét tuyển trực tuyến.
* Tích hợp ký số hoặc xác thực tài liệu nâng cao.
* Tích hợp OCR hỗ trợ đọc thông tin từ giấy tờ.
* Dashboard nâng cao theo thời gian thực.
* Đồng bộ dữ liệu trúng tuyển sang hệ thống quản lý sinh viên.
* Hỗ trợ nhiều nguyện vọng, lọc ảo và xác nhận nhập học trực tuyến nếu mô hình tuyển sinh của nhà trường yêu cầu.

## 13. Phụ lục

### 13.1. Danh sách tài liệu thường gặp trong hồ sơ tuyển sinh tại Việt Nam

* CCCD hoặc CMND.
* Học bạ.
* Bằng tốt nghiệp hoặc giấy chứng nhận tốt nghiệp tạm thời.
* Ảnh chân dung.
* Giấy tờ ưu tiên, nếu có.
* Các biểu mẫu và giấy tờ bổ sung do từng trường quy định.

### 13.2. Thuật ngữ nghiệp vụ phổ biến trong tuyển sinh tại Việt Nam

* Nguyện vọng.
* Phương thức xét tuyển.
* Tổ hợp xét tuyển.
* Điểm sàn.
* Điểm ưu tiên.
* Xác nhận nhập học.

### 13.3. Nguồn tham khảo nghiệp vụ

* VinUniversity Admissions.
* Hướng dẫn ứng tuyển First-year Applicants của VinUniversity.
* Thông tin tuyển sinh chính thức của International University.
* Bachelor Application Guideline của VGU.
* Tài liệu tổng quan về Software Requirements Specification.

### 13.4. Ghi chú về phạm vi tài liệu

Tài liệu này là bản mở rộng từ `srs.md`, được bổ sung thêm cấu trúc SRS đầy đủ hơn và nghiệp vụ tham khảo từ các cổng tuyển sinh công khai. Khi triển khai thực tế, nhà trường cần chốt thêm quy chế tuyển sinh nội bộ, biểu mẫu, luồng phê duyệt và danh sách báo cáo bắt buộc theo từng năm học.
