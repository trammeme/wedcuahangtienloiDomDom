<script type="text/javascript">
    $(document).ready(function () {
        $('#btnShowUserManagement').click(function () {
            $('#user-management-partial').toggle();
            $('#danh-gia-partial').hide(); // Ẩn phần đánh giá
        });

    $('#btnShowPromotions').click(function () {
        alert("Quản lý khuyến mại đang được phát triển!");
        });

    $('#btnShowPointManagement').click(function () {
        alert("Quản lý tích điểm đang được phát triển!");
        });

    $('#btnShowReviews').click(function () {
        $('#danh-gia-partial').toggle();
    $('#user-management-partial').hide(); // Ẩn phần người dùng
        });

    $('#btnLogout').click(function () {
        window.location.href = '@Url.Action("Logout", "Admin")';
        });
    });
</script>
function deleteUser(maKH) {
    if (confirm('Bạn có chắc chắn muốn xóa người dùng này?')) {
        $.ajax({
            url: '@Url.Action("Delete", "QLUser")',
            type: 'POST',
            data: { maKH: maKH },
            success: function (response) {
                if (response.success) {
                    alert('Người dùng đã được xóa thành công!');
                    location.reload(); // Tải lại trang để cập nhật danh sách người dùng
                } else {
                    alert('Lỗi: ' + response.message);
                }
            },
            error: function () {
                alert('Có lỗi xảy ra trong quá trình xóa người dùng.');
            }
        });
    }
}
