using DesafioTecnico1_Fundamentos.Controllers;
using DesafioTecnico1_Fundamentos.Domains;
using DesafioTecnico1_Fundamentos.Services;
using DesafioTecnico1_Fundamentos.Utils.Menus;
using System.Reflection;

TurmaController turmaController = new TurmaController();
AlunoController alunoController = new AlunoController();
DocenteController docenteController = new DocenteController();
MatriculaService matriculaService = new MatriculaService(turmaController, alunoController, docenteController);

MenuGerenciarTurma menuTurma = new MenuGerenciarTurma();
MenuGerenciarAluno menuAluno = new MenuGerenciarAluno();
MenuGerenciarDocente menuDocente = new MenuGerenciarDocente();
MenuSair menuSair = new MenuSair();

void ExibirOpcoesMenu()
{
    int opcaoSelecionada = 1;
    while (opcaoSelecionada != 0)
    {
        Console.Clear();
        Console.WriteLine("Sistema de Gestão de Turmas");
        Console.WriteLine("\nO que deseja fazer?");
        Console.WriteLine("1 - Gerenciar turma");
        Console.WriteLine("2 - Gerenciar Aluno");
        Console.WriteLine("3 - Gerenciar Docente");
        Console.WriteLine("0 - Sair");

        Console.WriteLine("\nSelecione uma opção: ");
        opcaoSelecionada = int.Parse(Console.ReadLine());

        switch (opcaoSelecionada)
        {
            case 1:               
                menuTurma.Exibir(turmaController, matriculaService,alunoController,docenteController);
                break;
            case 2:                
                menuAluno.Exibir(alunoController);
                break;
            case 3:                
                menuDocente.Exibir(docenteController);
                break;
            case 0:           
                menuSair.Exibir();
                break;
        }

    }
    
}
Aluno aluno1 = new Aluno(1, "Samuel");
Aluno aluno2 = new Aluno(2, "Pedro");
Aluno aluno3 = new Aluno(3, "Lucas");
Aluno aluno4 = new Aluno(4, "Gabriel");

alunoController.todosOsAlunos.Add(aluno1);
alunoController.todosOsAlunos.Add(aluno2);
alunoController.todosOsAlunos.Add(aluno3);
alunoController.todosOsAlunos.Add(aluno4);

Turma turmaA = new Turma(1, "TurmaA");
Turma turmaB = new Turma(2, "TurmaB");
Turma turmaC = new Turma(3, "TurmaC");

turmaController.todasAsTurmas.Add(turmaA);
turmaController.todasAsTurmas.Add(turmaB);
turmaController.todasAsTurmas.Add(turmaC);

Docente docente1 = new Docente(1, "Samuel");
Docente docente2 = new Docente(2, "Pedro");
Docente docente3 = new Docente(3, "Lucas");
Docente docente4 = new Docente(4, "Gabriel");

docenteController.todosOsDocentes.Add(docente1);
docenteController.todosOsDocentes.Add(docente2);
docenteController.todosOsDocentes.Add(docente3);
docenteController.todosOsDocentes.Add(docente4);

ExibirOpcoesMenu();
