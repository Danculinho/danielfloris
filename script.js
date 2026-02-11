const yesBtn = document.getElementById("yesBtn");
const noBtn = document.getElementById("noBtn");
const zone = document.getElementById("buttonZone");
const message = document.getElementById("message");
const card = document.getElementById("card");

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

function addEscapeAnimation() {
  noBtn.classList.remove("is-escaping");
  requestAnimationFrame(() => {
    noBtn.classList.add("is-escaping");
  });
}

function addFloatingHearts() {
  const hearts = ["💖", "💘", "💕", "✨", "🐧"];

  for (let i = 0; i < 14; i += 1) {
    const heart = document.createElement("span");
    heart.textContent = hearts[Math.floor(Math.random() * hearts.length)];
    heart.style.position = "fixed";
    heart.style.left = `${Math.random() * 100}vw`;
    heart.style.top = "102vh";
    heart.style.fontSize = `${Math.random() * 1 + 1.1}rem`;
    heart.style.zIndex = "999";
    heart.style.pointerEvents = "none";
    heart.style.transition = "transform 1500ms ease-out, opacity 1500ms ease-out";

    document.body.appendChild(heart);

    requestAnimationFrame(() => {
      const travelX = (Math.random() - 0.5) * 120;
      const travelY = -window.innerHeight - 120;
      heart.style.transform = `translate(${travelX}px, ${travelY}px) rotate(${Math.random() * 360}deg)`;
      heart.style.opacity = "0";
    });

    setTimeout(() => heart.remove(), 1600);
  }
}

function evadeNoButton(event) {
  event.preventDefault();
  noScale = Math.max(0.45, noScale - 0.07);
  setNoRandomPosition();
  addEscapeAnimation();

  const taunts = [
    "No no no... 🙈",
    "You can't catch me 😅",
    "I'm a shy button 💨",
    "Try pressing Yes instead 😘",
    "Smile and wave... and press YES 🐧",
  ];
  message.textContent = taunts[Math.floor(Math.random() * taunts.length)];
}

yesBtn.addEventListener("click", () => {
  message.textContent = "Yaaay! 💖 I can't wait for our Valentine's date! 🌹";
  noBtn.style.display = "none";
  yesBtn.textContent = "I love you 💞";
  yesBtn.style.transform = "scale(1.07)";

  card.classList.remove("celebrate");
  requestAnimationFrame(() => card.classList.add("celebrate"));
  addFloatingHearts();
});

noBtn.addEventListener("pointerenter", evadeNoButton);
noBtn.addEventListener("touchstart", evadeNoButton, { passive: false });
noBtn.addEventListener("click", evadeNoButton);
window.addEventListener("resize", setNoRandomPosition);

setNoRandomPosition();
