const yesBtn = document.getElementById("yesBtn");
const noBtn = document.getElementById("noBtn");
const zone = document.getElementById("buttonZone");
const message = document.getElementById("message");

let noScale = 1;

function setNoRandomPosition() {
  const zoneRect = zone.getBoundingClientRect();
  const noRect = noBtn.getBoundingClientRect();

  const maxX = Math.max(12, zoneRect.width - noRect.width - 12);
  const maxY = Math.max(12, zoneRect.height - noRect.height - 12);

  const x = Math.random() * maxX + 6;
  const y = Math.random() * maxY + 6;

  noBtn.style.left = `${x}px`;
  noBtn.style.top = `${y}px`;
  noBtn.style.transform = `scale(${noScale})`;
}

function evadeNoButton(event) {
  event.preventDefault();
  noScale = Math.max(0.45, noScale - 0.07);
  setNoRandomPosition();

  const taunts = [
    "No no no... 🙈",
    "You can't catch me 😅",
    "I'm a shy button 💨",
    "Try pressing Yes instead 😘",
  ];
  message.textContent = taunts[Math.floor(Math.random() * taunts.length)];
}

yesBtn.addEventListener("click", () => {
  message.textContent = "Yaaay! 💖 I can't wait for our Valentine's date! 🌹";
  noBtn.style.display = "none";
  yesBtn.textContent = "I love you 💞";
  yesBtn.style.transform = "scale(1.07)";
});

noBtn.addEventListener("pointerenter", evadeNoButton);
noBtn.addEventListener("touchstart", evadeNoButton, { passive: false });
noBtn.addEventListener("click", evadeNoButton);
window.addEventListener("resize", setNoRandomPosition);

setNoRandomPosition();
