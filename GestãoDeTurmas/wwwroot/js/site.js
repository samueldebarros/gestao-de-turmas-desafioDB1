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
