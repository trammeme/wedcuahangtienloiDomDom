document.addEventListener("DOMContentLoaded", function () {
    const domdomText = document.getElementById('domdomText'); // Lấy chữ DOMDOM
    const mainScreen = document.getElementById('screen'); // Màn hình chào đón
    const content = document.getElementById('content'); // Nội dung chính

    // Đợi một khoảng thời gian rồi làm chữ DOMDOM mất dần
    setTimeout(() => {
        domdomText.style.opacity = '0'; // Làm chữ DOMDOM mất dần
        setTimeout(() => {
            mainScreen.style.display = 'none'; // Ẩn màn hình chào đón
            content.style.display = 'flex'; // Hiển thị nội dung chính
            content.style.opacity = '1'; // Tăng độ mờ của nội dung chính
        }, 1000); // Đợi 1 giây trước khi ẩn màn hình
    }, 2000); // Đợi 2 giây trước khi làm chữ DOMDOM mất dần

    // Tìm kiếm khuyến mãi
    const searchBar = document.getElementById("search");
    if (searchBar) {
        searchBar.addEventListener("keyup", function () {
            let searchTerm = searchBar.value.toLowerCase();
            let promoItems = document.querySelectorAll(".promo-item");

            promoItems.forEach(function (item) {
                let promoText = item.textContent.toLowerCase();
                item.style.display = promoText.includes(searchTerm) ? "" : "none";
            });
        });
    }
});
$(document).ready(function () {
    var qrData = "https://example.com/tichdiem"; // Thay đổi thành dữ liệu bạn muốn mã hóa  
    $('#qr-code').qrcode({
        text: qrData,
        width: 100,  // chiều rộng của mã QR  
        height: 100  // chiều cao của mã QR  
    });
}); $(document).ready(function () {
    $('#imageCarousel').slick({
        dots: true,             // Hiển thị các chấm điều hướng
        infinite: true,         // Lướt vô hạn
        speed: 500,             // Tốc độ chuyển slide
        slidesToShow: 1,        // Hiển thị một hình mỗi lần
        slidesToScroll: 1,      // Cuộn từng hình một
        autoplay: true,         // Tự động lướt
        autoplaySpeed: 3000,    // Thời gian mỗi hình hiển thị (3000ms = 3s)
        arrows: true            // Hiển thị mũi tên điều hướng
    });
});
$(document).ready(function () {
    $('#imageCarousel').slick({
        dots: true,
        infinite: true,
        speed: 500,
        slidesToShow: 1,
        slidesToScroll: 1,
        autoplay: true,
        autoplaySpeed: 3000,
        arrows: true
    });
});
// script.js hoặc phần script cuối trang HTML  
$(document).ready(function () {
    // Hiện nội dung sau khi bấm vào "DOMDOM"  
    $('#domdomText').on('click', function () {
        $('#content').fadeIn();
        $('#screen').fadeOut();
    });

    // Hiển thị modal khi nhấn "Tích điểm"  
    $('.menu-item').eq(0).on('click', function () {
        $('#qr-modal').fadeIn(); // Hiện modal  

        // Tạo mã QR  
        $('#qr-code').empty(); // Đảm bảo kho ảnh trống trước khi tạo lại  
        $('#qr-code').qrcode({
            width: 128,
            height: 128,
            text: "https://example.com" // Thay đổi thành URL mà bạn muốn mã QR dẫn đến  
        });
    });

    // Đóng modal  
    $('.close-button').on('click', function () {
        $('#qr-modal').fadeOut();
    });

    // Đóng modal khi nhấn ra ngoài modal  
    $(window).on('click', function (event) {
        if (event.target.id === 'qr-modal') {
            $('#qr-modal').fadeOut();
        }
    });
}); function toggleReviewForm(button) {
    var reviewForm = button.nextElementSibling;
    reviewForm.style.display = reviewForm.style.display === 'none' ? 'block' : 'none';
}

document.querySelectorAll('.stars').forEach(function (stars) {
    stars.addEventListener('click', function (event) {
        var starElements = this.children;
        var rating = Array.from(starElements).indexOf(event.target) + 1;

        // Đánh dấu sao đã chọn  
        for (var i = 0; i < starElements.length; i++) {
            starElements[i].textContent = (i < rating) ? '⭐' : '☆';
        }
        this.setAttribute('data-rating', rating);
    });
});

function submitReview(button) {
    var reviewForm = button.parentElement;
    var rating = reviewForm.previousElementSibling.getAttribute('data-rating');
    var reviewText = reviewForm.querySelector('textarea').value;

    // Gửi dữ liệu đến máy chủ  
    $.ajax({
        type: "POST",
        url: "/path/to/your/api", // Thay thế đường dẫn này bằng URL thực tế của bạn  
        data: {
            rating: rating,
            review: reviewText
        },
        success: function (response) {
            alert("Đánh giá đã được gửi thành công!");
            // Bạn có thể đóng form hoặc xóa nội dung ở đây nếu cần  
            reviewForm.style.display = 'none';
            reviewForm.querySelector('textarea').value = '';
            reviewForm.previousElementSibling.setAttribute('data-rating', '0');
            var starElements = reviewForm.previousElementSibling.children;
            for (var i = 0; i < starElements.length; i++) {
                starElements[i].textContent = '☆';
            }
        },
        error: function () {
            alert("Có lỗi xảy ra, vui lòng thử lại.");
        }
    });
} $(document).ready(function () {
    // Khi nhấn vào nút trong menu
    $('.menu-item').on('click', function () {
        // Kiểm tra xem nút được nhấn có phải là "Khuyến mãi" không
        if ($(this).text() === 'Khuyến mãi') {
            // Ẩn phần sản phẩm và carousel
            $('#khuyenmai').hide();
            $('.carousel').hide(); // Ẩn phần ảnh lướt
            // Hiện phần khuyến mãi
            $('#cackhuyenmai').show(); // Sử dụng ID mới
        } else {
            // Nếu nhấn vào các menu khác, hiển thị sản phẩm và carousel
            $('#khuyenmai').show();
            $('.carousel').show(); // Hiện lại phần ảnh lướt
            $('#cackhuyenmai').hide(); // Sử dụng ID mới
        }
    });
});
