namespace LotoReg.Dtos
{
    public class RequerimentoResumoDto
    {
        public int Id { get; set; }
        public string Protocolo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
        public DateTime DataHora { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? NomeArquivo { get; set; }
    }
}
