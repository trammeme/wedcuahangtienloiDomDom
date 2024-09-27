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
