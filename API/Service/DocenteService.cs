using API.DTOs.DocenteDTOs;
using Common.Domains;
using Repository;

namespace API.Service;

public class DocenteService : IDocenteService
{
    private readonly IDocenteRepository _docenteRepository;

    public DocenteService(IDocenteRepository docenteRepository)
    {
        _docenteRepository = docenteRepository;
    }

    public async Task AdicionarDocenteAsync(DocenteInputDTO docente)
    {
        //botar verificação de CPF repetido-----------------

        Docente novoDocente = new Docente()
        {
            Nome = docente.Nome,
            Cpf = docente.Cpf,
            Email = docente.Email,
            Especialidade = docente.Especialidade,
            DataNascimento = docente.DataNascimento,
            Ativo = true
        };

        await _docenteRepository.AdicionarDocenteAsync(novoDocente);
    }
}
