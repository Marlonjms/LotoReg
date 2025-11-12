using LotoReg.Models;

namespace LotoReg.Interface
{
    public interface IVisaoGeral
    {
        Task<ModeloResposta> VisaoGeral(int idUsuario);
    }
}
