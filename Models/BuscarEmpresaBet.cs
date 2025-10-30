namespace LotoReg.Models
{
    public class BuscarEmpresaBet
    {
        // Campos obrigatórios
        public string RazaoSocial { get; set; } = null!;
        public string CNPJ { get; set; } = null!;
        public DateTime DataFundacao { get; set; }
        public string Estado { get; set; } = null!;
        public string EnderecoCompleto { get; set; } = null!;
        public string TelefoneComercial { get; set; } = null!;
        public string EmailContato { get; set; } = null!;


        public byte[]? ContratoSocialPdf { get; set; }

        // Campos opcionais
        public string? SitePlataforma { get; set; }

        // Sócios enviados como JSON

        public string? SociosJson { get; set; }

        // Sócios desserializados

        public List<SociosDto>? Socios { get; set; }
    }

    public class SociosDto
    {
        public string Nome { get; set; } = null!;
    }
}

