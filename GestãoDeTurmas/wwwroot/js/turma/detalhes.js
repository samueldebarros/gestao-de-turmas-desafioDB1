document.addEventListener('DOMContentLoaded', function () {

    const configElement = document.getElementById('detalhe-config');
    if (!configElement) return; 

    const config = configElement.dataset;
    const turmaId = parseInt(config.turmaId);

    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const tokenSeguranca = tokenInput ? tokenInput.value : '';

    const selectDisciplina = document.getElementById('select-disciplina');
    const selectDocente = document.getElementById('select-docente');
    const btnVincular = document.getElementById('btn-vincular');

    if (selectDisciplina && selectDocente) {
        selectDisciplina.addEventListener('change', async function () {
            const disciplinaId = this.value;

            selectDocente.innerHTML = '<option value="">Aguarde...</option>';
            selectDocente.disabled = true;

            if (!disciplinaId) {
                selectDocente.innerHTML = '<option value="">Selecione a disciplina primeiro...</option>';
                return;
            }

            try {
                const response = await fetch(`${config.urlDocentes}?disciplinaId=${disciplinaId}`);
                if (!response.ok) throw new Error("Erro na rede");

                const docentes = await response.json();

                selectDocente.innerHTML = '<option value="">Selecione o docente...</option>';
                docentes.forEach(d => {
                    selectDocente.innerHTML += `<option value="${d.id}">${d.nome}</option>`;
                });

                selectDocente.disabled = false;
            } catch (error) {
                console.error("Erro ao carregar docentes:", error);
                selectDocente.innerHTML = '<option value="">Erro ao buscar opções.</option>';
            }
        });
    }

    if (btnVincular) {
        btnVincular.addEventListener('click', async function () {
            const disciplinaId = selectDisciplina.value;
            const docenteId = selectDocente.value;

            if (!disciplinaId || !docenteId) {
                alert("Por favor, selecione a disciplina e o docente.");
                return;
            }

            btnVincular.disabled = true;
            const textoOriginal = btnVincular.innerHTML;
            btnVincular.innerHTML = "Vinculando...";

            try {
                const formData = new FormData();
                formData.append("TurmaId", turmaId);
                formData.append("DisciplinaId", disciplinaId);
                formData.append("DocenteId", docenteId);
                formData.append("__RequestVerificationToken", tokenSeguranca);

                const response = await fetch(config.urlVincular, {
                    method: 'POST',
                    body: formData 
                });

                const jsonResposta = await response.json();

                if (jsonResposta.sucesso) {
                    location.reload();
                } else {
                    btnVincular.disabled = false;
                    btnVincular.innerHTML = textoOriginal;
                }
            } catch (error) {
                alert("Erro de comunicação com o servidor.");
                btnVincular.disabled = false;
                btnVincular.innerHTML = textoOriginal;
            }
        });
    }

});