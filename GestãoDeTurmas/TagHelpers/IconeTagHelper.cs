using Common.Utils;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GestaoTurmas.TagHelpers
{
    [HtmlTargetElement("icone")]
    public class IconeTagHelper : TagHelper
    {
        public TipoIcone Nome { get; set; }

        public string Cor { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "i";

            output.TagMode = TagMode.StartTagAndEndTag;

            var classeIcone = Nome switch
            {
                TipoIcone.Usuarios => "bi bi-people",
                TipoIcone.Disciplinas => "bi bi-book",
                TipoIcone.Editar => "bi bi-pencil",
                TipoIcone.SetaDireita => "bi bi-arrow-right",
                TipoIcone.SetaEsquerda => "bi bi-arrow-left",
                TipoIcone.Lixeira => "bi bi-trash",
                TipoIcone.Pesquisar => "bi bi-search",
                TipoIcone.Adicionar => "bi bi-plus-lg",
                TipoIcone.Casa => "bi bi-house",
                TipoIcone.Docentes => "bi bi-person-badge",
                TipoIcone.Turmas => "bi bi-building",
                TipoIcone.Filtrar => "bi bi-funnel",
                TipoIcone.Sair => "bi bi-box-arrow-right",
                _ => "bi bi-question-circle"
            };

            output.Attributes.Add("class", classeIcone);

            if (!string.IsNullOrEmpty(Cor))
            {
                output.Attributes.Add("style", $"color: {Cor};");
            }
        }
    }
}