using LotoReg.Dtos;

namespace LotoReg.Interface
{
    public interface IRequerimentoBet
    {
        Task EnviarRequerimento(EnviarRequerimentoDto dto, int usuarioId);
        Task<List<RequerimentoResumoDto>> ObterTodosRequerimentos(int usuarioId);
        Task CadastrarModeloRequerimento(ModeloRequerimentoDto dto);
        Task<List<ModeloRequerimentoDownloadDto>> ObterTodosModelos();
        Task<BaixarModeloRequerimentoDto> BaixarModeloPorId(int id);
        Task DeletarModeloPorId(int id);
    }
}
