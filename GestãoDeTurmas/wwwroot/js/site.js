const modal = new bootstrap.Modal(document.getElementById("modal-formulario"));
const modalTitulo = document.getElementById("modal-titulo");
const modalBody = document.getElementById("modal-body");

async function abrirModal(url, titulo) {
    modalTitulo.innerText = titulo;
    modalBody.innerHTML = "<span>Carregando...</span>";
    modal.show();

    const resposta = await fetch(url);
    modalBody.innerHTML = await resposta.text();
}

async function abrirModalConfirmacao(url, titulo) {
    modalTitulo.innerText = titulo;
    modalBody.innerHTML = "<span>Carregando...</span>";
    modal.show();

    modalBody.innerHTML =
        `
        <div style="display: flex; flex-direction: column; gap: 16px;">
            <p>Tem certeza?</p>
            <div style="display: flex; justify-content: flex-end; gap: 8px;">
                <button class="botao botao-secundario" data-bs-dismiss="modal">Cancelar</button>
                <button class="botao botao-perigo" onclick="onConfirmar('${url}')">Confirmar</button>
            </div>
        </div>
    `;
}

async function onConfirmar(url) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    const resposta = await fetch(url, {
        method: "POST",
        headers: {
            "RequestVerificationToken" : token
        }
    });
    if (resposta.ok) {
        modal.hide();
        location.reload();
    } else {
        const erro = await resposta.json();
        modalBody.innerHTML = `<div class="alerta alerta-erro">${erro.mensagem}</div>`;
    }
}

document.getElementById('modal-formulario').addEventListener('submit', async (event) => {
    event.preventDefault();

    const form = event.target;
    const urlForm = form.action;
    const payload = new URLSearchParams(new FormData(form));

    try {
        const respostaForm = await fetch(urlForm, {
            method: 'POST',
            body: payload
        });

        if (respostaForm.ok) {
            modal.hide();
            location.reload();
            return;
        }

        if (respostaForm.status === 400) {
            modalBody.innerHTML = await respostaForm.text();
            return;
        }

        if (respostaForm.status === 404) {
            const json = await respostaForm.json();
            modalBody.innerHTML = `<div class="alerta alerta-erro">${json.mensagem}</div>`;
            return;
        }

        throw new Error("Erro de processamento no servidor");

    } catch (erro) {
        modalBody.innerHTML = `<div class="alerta alerta-erro">Falha ao comunicar com o servidor: ${erro.message}</div>`;
    }
})



// Modal de confirmação - a fazer
/* 
<div class="modal fade" id="modal-confirmacao" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="modal-confirmacao-titulo"></h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Fechar"></button>
            </div>
            <div class="modal-body" id="modal-confirmacao-body">
            </div>
        </div>
    </div>
</div>

<div class="modal fade" id="modal-formulario" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="modal-titulo"></h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Fechar"></button>
            </div>
            <div class="modal-body" id="modal-body">
            </div>
        </div>
    </div>
</div>
*/
