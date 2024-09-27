function createSnowflakes() {
    for (let i = 0; i < 25; i++) { // Giảm số lượng tuyết xuống 25
        let snowflake = document.createElement("div");
        snowflake.className = "snowflake";
        snowflake.innerHTML = "&#10052;";
        snowflake.style.left = Math.random() * 100 + "vw"; 
        snowflake.style.animationDuration = Math.random() * 3 + 2 + "s"; 
        snowflake.style.fontSize = Math.random() * 10 + 10 + "px"; 
        document.body.appendChild(snowflake);

        setTimeout(() => {
            snowflake.remove();
        }, 5000);
    }
}

setInterval(createSnowflakes, 500);