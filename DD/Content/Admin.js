$(document).ready(function () {
    $('.nav-link').on('click', function () {
        // Xóa lớp 'active' khỏi tất cả các liên kết
        $('.nav-link').removeClass('active');
        // Thêm lớp 'active' vào liên kết đang được nhấn
        $(this).addClass('active');
    });
});
