function showEditModal(maKH) {
    console.log("Bấm nút sửa cho maKH:", maKH);
    $.get('@Url.Action("Edit", "QLUser")', { maKH: maKH }, function (data) {
        $('#editMaKH').val(data.MaKH);
        $('#editTenKH').val(data.TenKH);
        $('#editEmail').val(data.Email);
        // Thêm các trường khác tương tự ở đây

        $('#editUserModal').show();
    }).fail(function () {
        alert('Không thể lấy dữ liệu người dùng.');
    });
}

function closeModal() {
    $('#editUserModal').hide();
}

$('#editUserForm').on('submit', function (e) {
    e.preventDefault();

    $.post($(this).attr('action'), $(this).serialize(), function (result) {
        if (result.success) {
            alert('Cập nhật thành công!');
            location.reload();
        } else {
            alert('Có lỗi xảy ra: ' + result.message);
        }
    });
});
function deleteUser(maKH) {
    if (confirm('Bạn có chắc chắn muốn xóa người dùng này?')) {
        $.ajax({
            url: '@Url.Action("Delete", "QLUser")',
            type: 'POST',
            data: { maKH: maKH },
            success: function (response) {
                if (response.success) {
                    alert('Người dùng đã được xóa thành công!');
                    location.reload();
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
