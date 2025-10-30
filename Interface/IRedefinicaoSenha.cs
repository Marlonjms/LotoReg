using LotoReg.Dtos;

namespace LotoReg.Interface
{
    public interface IRedefinicaoSenha
    {
        Task EnviarEmailRedefinicao(EditarSenhaRequestDto dto);
        Task <bool> ValidarToken (string token);
        Task AtualizarSenha(string token, string senha);

    }
}
