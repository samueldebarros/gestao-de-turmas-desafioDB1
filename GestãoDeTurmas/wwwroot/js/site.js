// Modal de Input
const existeModal = document.getElementById('modal-formulario');
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
        } else {
            const alertaErro = `<div class="alert alert-danger alert-dismissible fade show" role="alert">
                              ${json.mensagem}
                              <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                           </div>`;

            modalBody.insertAdjacentHTML('afterbegin', alertaErro);
        }
    } else {
        modalBody.innerHTML = await response.text();

        const campoCpf = modalBody.querySelector('.mascara-cpf');
        if (campoCpf) {
            $(campoCpf).mask('000.000.000-00');
        }
    }
});
//--------------------------------------------------
// Modal de confirmação

document.addEventListener('DOMContentLoaded', function () {
    const modalConfirmacao = new bootstrap.Modal(document.getElementById('modal-confirmacao'));
    const modalConfirmacaoTitulo = document.getElementById('modal-confirmacao-titulo');
    const modalConfirmacaoMensagem = document.getElementById('modal-confirmacao-mensagem');
    const modalConfirmacaoBtn = document.getElementById('modal-confirmacao-btn');

    window.confirmar = function (titulo, mensagem, onConfirmar, tipoBotao = 'botao-perigo') {
        modalConfirmacaoTitulo.textContent = titulo;
        modalConfirmacaoMensagem.textContent = mensagem;

        modalConfirmacaoBtn.className = `botao ${tipoBotao}`;

        modalConfirmacaoBtn.onclick = () => {
            modalConfirmacao.hide();
            onConfirmar();
        };

        modalConfirmacao.show();
    };
});
