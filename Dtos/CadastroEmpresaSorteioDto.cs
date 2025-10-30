

namespace LotoReg.Dtos
{
    public class CadastroEmpresaSorteioDto
    {
        // Campos obrigatórios
        public string RazaoSocial { get; set; } = null!;
        public string CNPJ { get; set; } = null!;
        public DateTime DataFundacao { get; set; }
        public string Estado { get; set; } = null!;
        public string EnderecoCompleto { get; set; } = null!;
        public string TelefoneComercial { get; set; } = null!;
        public string EmailContato { get; set; } = null!;


        // Campos opcionais
        public string? SitePlataforma { get; set; }

        // Sócios enviados como JSON

        public string? SociosJson { get; set; }

        // Sócios desserializados

        public List<SocioEmpresaSorteioDto>? Socios { get; set; }
    }

    public class SocioEmpresaSorteioDto
    {
        public string Nome { get; set; } = null!;
    }
}
