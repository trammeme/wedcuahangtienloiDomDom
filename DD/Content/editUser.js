<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
<script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>

function editUser(maKH) {
    $.get('@Url.Action("Edit", "QLUser")', { maKH: maKH }, function (data) {
        $('#editMaKH').val(data.MaKH);
        $('#editTenKH').val(data.TenKH);
        $('#editEmail').val(data.Email);
        $('#editGioiTinh').val(data.GioiTinh);
        $('#editThangSinh').val(data.ThangSinh);
        $('#editDiaChi').val(data.DiaChi);
        $('#editSoDienThoai').val(data.SoDienThoai);
        $('#editTenDangNhap').val(data.TenDangNhap);
        $('#editUserModal').modal('show');
    });
}
