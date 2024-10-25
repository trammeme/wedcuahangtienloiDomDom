function createSnowflakes() {
    const snowflakes = '❅❆✻❄';
    const container = document.body;
    const numberOfSnowflakes = 30;

    for (let i = 0; i < numberOfSnowflakes; i++) {
        const snowflake = document.createElement('div');
        const randomChar = snowflakes[Math.floor(Math.random() * snowflakes.length)];

        snowflake.className = 'snowflake';
        snowflake.style.left = `${Math.random() * 100}%`;
        snowflake.style.opacity = Math.random();
        snowflake.style.fontSize = `${Math.random() * 10 + 10}px`;
        snowflake.innerHTML = randomChar;

        snowflake.style.animationDelay = `${Math.random() * 10}s, ${Math.random() * 10}s`;

        container.appendChild(snowflake);
    }
}

document.addEventListener('DOMContentLoaded', function () {
    createSnowflakes();

    setTimeout(() => {
        document.querySelector('.welcome-text').classList.add('glow');
        setTimeout(() => {
            document.querySelector('.welcome-text').classList.remove('glow');
        }, 1000);
    }, 3000);
});