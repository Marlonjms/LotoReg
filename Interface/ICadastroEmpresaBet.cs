using LotoReg.Dtos;
using LotoReg.Models;


namespace LotoReg.Interface
{
    public interface ICadastroEmpresaBet
    {
        Task CadastrarEmpresa(CadastroEmpresaBetDto empresa, byte[] contratoSocialPdf, int idUsuario);
        Task AtualizarEmpresa(int usuarioId, AtualizarEmpresaBetDto dto);
        Task<BuscarEmpresaBet?> ObterEmpresa(int idUsuario);
    }
}
