using System.Net;
using System.Net.Mail;
using Npgsql;
using LotoReg.Interface;
using LotoReg.Serviços;

namespace LotoReg.Service
{
    public class VerificacaoEmailServico : IVerificacaoEmail
    {
        private readonly string _stringConexao;
        private readonly GmailServico _gmailServico;

        public VerificacaoEmailServico(IConfiguration config, GmailServico gmailServico)
        {
            _stringConexao = config.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("DefaultConnection", "A string de conexão não foi configurada.");
            _gmailServico = gmailServico;
        }

        public async Task EnviarCodigoVerificacao(string email)
        {
            // 1️⃣ Valida formato do e-mail
            try
            {
                var endereco = new MailAddress(email);
                email = endereco.Address; // normaliza o formato
            }
            catch
            {
                throw new ArgumentException("E-mail inválido. Verifique o endereço digitado.");
            }

            // 2️⃣ Valida se o domínio existe
            string dominio = email.Split('@').Last();
            try
            {
                await Dns.GetHostEntryAsync(dominio);
            }
            catch
            {
                throw new ArgumentException("O domínio do e-mail não existe. Verifique o endereço digitado.");
            }

            // 3️⃣ Gera o código aleatório
            string codigo = new Random().Next(100000, 999999).ToString();

            await using var conexao = new NpgsqlConnection(_stringConexao);
            await conexao.OpenAsync();

            // 4️⃣ Verifica se o e-mail já existe no sistema
            var cmdVerifica = new NpgsqlCommand("SELECT COUNT(*) FROM usuarios WHERE email = @Email", conexao);
            cmdVerifica.Parameters.AddWithValue("@Email", email);
            int count = Convert.ToInt32(await cmdVerifica.ExecuteScalarAsync());

            if (count > 0)
                throw new ArgumentException("Já existe um usuário cadastrado com este e-mail.");

            // 5️⃣ Insere ou atualiza na tabela de verificação
            var cmdInsert = new NpgsqlCommand(@"
                INSERT INTO verificacao_email (email, codigo, validade, confirmado)
                VALUES (@Email, @Codigo, @Validade, false)
                ON CONFLICT (email)
                DO UPDATE SET codigo = @Codigo, validade = @Validade, confirmado = false;
            ", conexao);

            cmdInsert.Parameters.AddWithValue("@Email", email);
            cmdInsert.Parameters.AddWithValue("@Codigo", codigo);
            cmdInsert.Parameters.AddWithValue("@Validade", DateTime.Now.AddMinutes(15));
            await cmdInsert.ExecuteNonQueryAsync();

            // 6️⃣ Envia o e-mail
            try
            {
                await _gmailServico.EnviarEmailAsync(
                    email,
                    "Código de verificação",
                    $"Seu código de verificação é: {codigo}"
                );
            }
            catch
            {
                throw new Exception("Não foi possível enviar o e-mail. Verifique se o endereço existe ou está correto.");
            }
        }

        public async Task<bool> ConfirmarCodigo(string email, string codigo)
        {
            email = email.Trim();
            codigo = codigo.Trim();

            await using var conexao = new NpgsqlConnection(_stringConexao);
            await conexao.OpenAsync();

            var comando = new NpgsqlCommand("SELECT validade FROM verificacao_email WHERE email = @Email AND codigo = @Codigo", conexao);
            comando.Parameters.AddWithValue("@Email", email);
            comando.Parameters.AddWithValue("@Codigo", codigo);

            var validadeObj = await comando.ExecuteScalarAsync();
            if (validadeObj == null)
                return false;

            if (!DateTime.TryParse(validadeObj.ToString(), out DateTime validade))
                return false;

            if (validade < DateTime.Now)
                return false;

            var comandoUpdate = new NpgsqlCommand("UPDATE verificacao_email SET confirmado = true WHERE email = @Email", conexao);
            comandoUpdate.Parameters.AddWithValue("@Email", email);
            await comandoUpdate.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<bool> EstaConfirmado(string email)
        {
            await using var conexao = new NpgsqlConnection(_stringConexao);
            await conexao.OpenAsync();

            var comando = new NpgsqlCommand("SELECT confirmado FROM verificacao_email WHERE email = @Email", conexao);
            comando.Parameters.AddWithValue("@Email", email);
            var resultado = await comando.ExecuteScalarAsync();

            return resultado != null && Convert.ToBoolean(resultado);
        }
    }
}
