// reloj.js
function actualizarReloj() {
    const ahora = new Date();

    const dia = String(ahora.getDate()).padStart(2, '0');
    const mes = String(ahora.getMonth() + 1).padStart(2, '0');
    const anio = ahora.getFullYear();
    const horas = String(ahora.getHours()).padStart(2, '0');
    const minutos = String(ahora.getMinutes()).padStart(2, '0');

    const reloj = document.getElementById("reloj");
    if (reloj) {
        reloj.textContent = `${dia}/${mes}/${anio} ${horas}:${minutos}`;
    }
}

setInterval(actualizarReloj, 1000);
document.addEventListener("DOMContentLoaded", actualizarReloj);
