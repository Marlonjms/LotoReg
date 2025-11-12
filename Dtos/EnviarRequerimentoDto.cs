

namespace LotoReg.Dtos
{
    public class EnviarRequerimentoDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
        public IFormFile? Documento { get; set; }
    }
}
