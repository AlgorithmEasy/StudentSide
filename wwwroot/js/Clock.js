Clock = {}

Clock.ShowTime = () => {
    const time = new Date().toLocaleTimeString("en-US", { hour: "numeric", hour12: false, minute: "numeric" });
    const clock = document.getElementById("clock");
    if (clock) {
        clock.innerText = time;
    }
}

setInterval(Clock.ShowTime, 1000);