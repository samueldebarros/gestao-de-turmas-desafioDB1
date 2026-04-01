using Common.Utils;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GestaoTurmas.TagHelpers
{
    [HtmlTargetElement("icone")]
    public class IconeTagHelper : TagHelper
    {
        public TipoIcone Nome { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "i";

            output.TagMode = TagMode.StartTagAndEndTag;

            var classeIcone = Nome switch
            {
                TipoIcone.Alunos => "bi bi-mortarboard",
                TipoIcone.Disciplinas => "bi bi-book",
                TipoIcone.Editar => "bi bi-pencil",
                TipoIcone.SetaDireita => "bi bi-arrow-right",
                TipoIcone.SetaEsquerda => "bi bi-arrow-left",
                TipoIcone.SetaCima => "bi bi-arrow-up",
                TipoIcone.SetaBaixo => "bi bi-arrow-down",
                TipoIcone.Lixeira => "bi bi-trash",
                TipoIcone.Pesquisar => "bi bi-search",
                TipoIcone.Adicionar => "bi bi-plus-lg",
                TipoIcone.Casa => "bi bi-house",
                TipoIcone.Docentes => "bi bi-person-workspace",
                TipoIcone.Turmas => "bi bi-people",
                TipoIcone.Filtrar => "bi bi-funnel",
                TipoIcone.Aviso => "bi bi-exclamation-triangle",
                TipoIcone.Sair => "bi bi-box-arrow-right",
                TipoIcone.Caneta => "bi bi-pencil",
                TipoIcone.Inativar => "bi bi-slash-circle",
                TipoIcone.Reativar => "bi bi-arrow-counterclockwise",
                TipoIcone.Check => "bi bi-check-lg",
                _ => "bi bi-question-circle"
            };

            output.Attributes.Add("class", classeIcone);

        }
    }
}