using LotoReg.Dtos;
using LotoReg.Models;

namespace LotoReg.Interface
{
    public interface IUsuario
    {
        Task <BuscarUsuarioModelo?> GetUsuario(int IdUsuario);
        Task  CadastrarUsuario(CadastroUsuarioDto cadastroUsuarioDto);
        Task EditarUsuario(int id, EditarUsuarioDto dto);
        Task DeletarUsuario(int idUsuario);
    }
}
