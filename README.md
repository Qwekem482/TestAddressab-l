# NavMeshObstacle Replacer Tool

## Giải thích vấn đề
Tool NvMObsReplacer trước đây có lỗi khi chạy `Process(false, false)`:
- Không remove NavMeshObstacle components khi shouldRemove = false (đây là hành vi đúng)
- Chỉ replace/process NavMeshObstacle trong 1 object thay vì tất cả objects trong scene

## Giải pháp đã áp dụng
1. **Xử lý nhiều objects**: Sử dụng `FindObjectsOfType<NavMeshObstacle>()` để tìm TẤT CẢ NavMeshObstacle components trong scene, không chỉ một object
2. **Logic xử lý đúng**:
   - `shouldRemove = false`: Không remove components (giữ nguyên)
   - `shouldReplace = false`: Không thực hiện replacement
   - `Process(false, false)`: Chỉ log thông tin, không thay đổi gì
3. **Logging chi tiết**: Tạo log file với timestamp `NvMObsReplacer_ReplaceLog_YYYYMMDD_HHMMSS.txt`

## Cách sử dụng
1. Mở Unity Editor
2. Vào menu `Tools > NavMeshObstacle Replacer`
3. Click button tương ứng với tham số mong muốn:
   - `Process (Remove: true, Replace: true)`: Remove và replace components
   - `Process (Remove: true, Replace: false)`: Chỉ remove components
   - `Process (Remove: false, Replace: true)`: Chỉ modify existing components
   - `Process (Remove: false, Replace: false)`: Chỉ log thông tin, không thay đổi

## Test
Sử dụng menu `Tools > Create Test NavMeshObstacles` để tạo test objects, sau đó test tool với các tham số khác nhau.

## Files
- `NvMObsReplacer.cs`: Tool chính
- `TestNavMeshSetup.cs`: Utility để tạo test objects
