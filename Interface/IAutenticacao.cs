using LotoReg.Dtos;
namespace LotoReg.Interface
{
    public interface IAutenticacao
    {
        Task<string> Logar(LoginUsuarioDto loginDto);
    }
}