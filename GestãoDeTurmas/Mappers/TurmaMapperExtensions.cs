using API.DTOs.TurmaDTOs;
using Common.Domains;
using Common.Utils;
using GestãoDeTurmas.Models.Turma;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestãoDeTurmas.Mappers;

public static class TurmaMapperExtensions
{
    public static TurmaListaViewModel ToListaViewModel(this Turma turma)
    {
        return new TurmaListaViewModel
        {
            Id = turma.Id,
            Identificador = turma.Identificador,
            AnoLetivo = turma.AnoLetivo,
            Ativo = turma.Ativo,
            Serie = turma.Serie,
            Turno = turma.Turno,
            TotalAlunos = turma.Enturmamentos?.Count ?? 0,
            TotalDisciplinas = turma.GradeCurricular?.Count ?? 0,
            Capacidade = turma.Capacidade
        };
    }

    public static TurmaInputDTO ToInputDTO(this TurmaInputViewModel model)
    {
        return new TurmaInputDTO
        {
            Identificador = model.Identificador,
            AnoLetivo = model.AnoLetivo,
            Serie = model.Serie,
            Turno = model.Turno,
            Capacidade = model.Capacidade
        };
    }

    public static TurmaEditarViewModel ToEditarViewModel(this Turma turma)
    {
        return new TurmaEditarViewModel
        {
            Id = turma.Id,
            Identificador = turma.Identificador,
            AnoLetivo = turma.AnoLetivo,
            Serie = turma.Serie,
            Turno = turma.Turno,
            Capacidade = turma.Capacidade,
        };
    }

    public static EditarTurmaDTO ToEditarDTO(this TurmaEditarViewModel turma)
    {
        return new EditarTurmaDTO
        {
            Id = turma.Id,
            Identificador = turma.Identificador,
            AnoLetivo = turma.AnoLetivo,
            Serie = turma.Serie,
            Turno = turma.Turno,
            Capacidade = turma.Capacidade,
        };
    }

    public static TurmaDetalhesViewModel ToDetalhesViewModel(
    this Turma turma,
    List<Aluno> alunosDisponiveis,
    List<Disciplina> disciplinasDisponiveis)
    {
        return new TurmaDetalhesViewModel
        {
            Id = turma.Id,
            NomeExibicao = $"{turma.Serie.ObterDescricao()} {turma.Identificador}",
            Turno = turma.Turno.ObterTurnoTranscrito(),
            AnoLetivo = turma.AnoLetivo,
            Capacidade = turma.Capacidade,
            TotalMatriculados = turma.Enturmamentos?.Count ?? 0,
            Ativo = turma.Ativo,

            Alunos = turma.Enturmamentos?.Select(e => new EnturmamentoViewModel
            {
                AlunoId = e.AlunoId,
                Nome = e.Aluno.Nome,
                Matricula = e.Aluno.Matricula,
                Cpf = e.Aluno.Cpf.FormatarCpf(),
                Situacao = e.Situacao
            }).ToList() ?? new(),

            Grade = turma.GradeCurricular?.Select(g => new GradeCurricularViewModel
            {
                DisciplinaId = g.DisciplinaId,
                DocenteId = g.DocenteId,
                Disciplina = g.Disciplina.Nome,
                Docente = g.Docente.Nome
            }).ToList() ?? new(),

            AlunosDisponiveis = alunosDisponiveis.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Nome} — {a.Matricula}"
            }).ToList(),

            DisciplinasDisponiveis = disciplinasDisponiveis.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Nome
            }).ToList()
        };
    }
}
