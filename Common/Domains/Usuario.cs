
namespace Common.Domains;

public class Usuario : IEntidade
{
    public int Id { get; set; }
    public string Email { get;set;}
    public string SenhaHash { get; set; }
    public string Role { get; set; }
}
