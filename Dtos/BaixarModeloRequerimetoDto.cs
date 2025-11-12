namespace LotoReg.Dtos
{
    public class BaixarModeloRequerimentoDto
    {
        public string Nome { get; set; } = string.Empty;
        public byte[] Arquivo { get; set; } = [];
    }
}
