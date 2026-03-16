// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const modal = new bootstrap.Modal(document.getElementById('modal-formulario'));
const modalBody = document.getElementById('modal-body');
const modalTitulo = document.getElementById('modal-titulo');

async function abrirModal(url, titulo) {
    modalTitulo.textContent = titulo;
    modalBody.innerHTML = '<p class="text-center">Carregando...</p>';
    modal.show();

    const response = await fetch(url);
    modalBody.innerHTML = await response.text();

    const campoCpf = modalBody.querySelector('.mascara-cpf');
    if (campoCpf) {
        $(campoCpf).mask('000.000.000-00');
    }
}

document.getElementById('modal-formulario').addEventListener('submit', async function (e) {
    e.preventDefault();

    const form = e.target.closest('form');
    const url = form.action;
    const formData = new FormData(form);

    const response = await fetch(url, {
        method: 'POST',
        body: formData
    });

    const contentType = response.headers.get('content-type');

    if (contentType && contentType.includes('application/json')) {
        const json = await response.json();
        if (json.sucesso) {
            modal.hide();
            location.reload();
        }
    } else {
        modalBody.innerHTML = await response.text();
    }
});