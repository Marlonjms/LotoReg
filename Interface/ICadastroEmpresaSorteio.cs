using LotoReg.Dtos;
using LotoReg.Models;

namespace LotoReg.Interface
{
    public interface ICadastroEmpresaSorteio
    {
        Task CadastrarEmpresa(CadastroEmpresaSorteioDto empresa, byte[] contratoSocialPdf, int idUsuario);
        Task AtualizarEmpresa(int idUsuario, AtualizarEmpresaSorteioDto dto);
        Task<BuscarEmpresaSorteio?> ObterEmpresa(int idUsuario);
    }
}
