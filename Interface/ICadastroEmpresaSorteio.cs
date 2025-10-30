using LotoReg.Dtos;
using LotoReg.Models;

namespace LotoReg.Interface
{
    public interface ICadastroEmpresaSorteio
    {
        Task CadastrarEmpresa(CadastroEmpresaSorteioDto empresa, byte[] contratoSocialPdf);
        Task AtualizarEmpresa(int idEmpresaSorteio, AtualizarEmpresaSorteioDto dto);
        Task<BuscarEmpresaSorteio?> ObterEmpresa(int idEmpresaSorteio);
    }
}
