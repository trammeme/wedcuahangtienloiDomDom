window.onload = function () {
    const firefly = document.getElementById('firefly');

    // Đặt vị trí ban đầu của đom đóm bên ngoài màn hình  
    firefly.style.left = Math.random() * 100 + 'vw';
    firefly.style.bottom = '100%';

    // Khi hoạt hình kết thúc, kéo màn hình lên để hiển thị nội dung  
    firefly.addEventListener('animationend', () => {
        setTimeout(() => {
            document.getElementById('screen').style.transition = 'transform 1s ease';
            document.getElementById('screen').style.transform = 'translateY(-100%)';
            setTimeout(() => {
                document.getElementById('screen').style.display = 'none';
                document.getElementById('content').style.display = 'flex';
            }, 1000);
        }, 500); // Một chút thời gian trước khi kéo lên  
    });
};