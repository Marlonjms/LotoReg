namespace LotoReg.Dtos
{
    public class ModeloRequerimentoDto
    {
        
        public string Nome { get; set; } = string.Empty;
        public IFormFile? Arquivo { get; set; }
    }

    
}
