window.onload = function () {
    const welcomeText = document.querySelector('.cute-text');
    const menu = document.getElementById('menu');
    const aboutLink = document.getElementById('about-link');

    // Hiện chữ "Welcome to DomDom!" dần dần  
    welcomeText.style.opacity = 1;
    welcomeText.style.transform = 'translateY(0)';

    // Tạo đom đóm  
    const firefly = document.createElement('span');
    firefly.classList.add('firefly');
    firefly.innerHTML = '✨';
    document.body.appendChild(firefly);

    // Lấy vị trí dấu chấm than trong dòng chữ  
    const textBounds = welcomeText.getBoundingClientRect();
    const exclamationX = textBounds.left + textBounds.width - 30;
    const exclamationY = textBounds.top + textBounds.height - 30;

    // Đặt vị trí khởi đầu cho đom đóm tại dấu chấm than  
    firefly.style.position = 'absolute';
    firefly.style.top = `${exclamationY}px`;
    firefly.style.left = `${exclamationX}px`;

    const targetX = window.innerWidth - 50;
    const targetY = 20;

    setTimeout(() => {
        firefly.style.transition = 'transform 2s ease-in-out, opacity 1s ease-in-out';
        firefly.style.transform = `translate(${targetX - exclamationX}px, ${targetY - exclamationY}px) rotate(360deg)`;
        firefly.style.opacity = '0';

        setTimeout(() => {
            menu.classList.add('visible');
            firefly.remove();

            // Gọi hàm tạo tuyết rơi  
            snowFall();
        }, 2000);
    }, 1000);

    // Hiệu ứng nhấp nháy cho đom đóm  
    setInterval(() => {
        firefly.style.opacity = firefly.style.opacity === '1' ? '0.5' : '1';
    }, 500);

    // Hàm tạo hiệu ứng tuyết rơi  
    function snowFall() {
        setInterval(createSnowflake, 200); // Tạo bông tuyết mỗi 200ms  
    }

    function createSnowflake() {
        const snowflake = document.createElement('span');
        snowflake.classList.add('snowflake');
        snowflake.innerHTML = '❄️'; // Bông tuyết  
        document.body.appendChild(snowflake);

        // Thay đổi vị trí bông tuyết  
        const x = Math.random() * window.innerWidth;
        snowflake.style.left = `${x}px`;
        snowflake.style.animationDuration = `${Math.random() * 10 + 5}s`; // Thời gian ngẫu nhiên từ 5 đến 15 giây  
        snowflake.style.opacity = Math.random(); // Độ mờ bông tuyết  

        // Thêm một sự kiện để xóa bông tuyết khi ra ngoài màn hình  
        snowflake.addEventListener('animationend', () => {
            snowflake.remove();
        });
    }
};// Lập trình lướt xuống  
