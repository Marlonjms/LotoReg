using Npgsql;
using System.Security.Cryptography;
using LotoReg.Dtos;
using LotoReg.Interface;
using LotoReg.Serviços;

namespace LotoReg.Service
{
    public class RedefinicaoSenhaServico : IRedefinicaoSenha
    {
        private readonly string _stringConexao;
        private readonly IConfiguration _configuracao;
        private readonly GmailServico _gmailServico;

        public RedefinicaoSenhaServico(IConfiguration configuracao, GmailServico gmailServico)
        {
            _configuracao = configuracao;
            _gmailServico = gmailServico;
            _stringConexao = configuracao.GetConnectionString("DefaultConnection")!;
        }

        public async Task EnviarEmailRedefinicao(EditarSenhaRequestDto dto)
        {
            string token = GerarToken();

            await using (var conexao = new NpgsqlConnection(_stringConexao))
            {
                await conexao.OpenAsync();

                // Verifica se o e-mail existe
                var comandoVerifica = new NpgsqlCommand("SELECT COUNT(*) FROM usuarios WHERE email = @Email", conexao);
                comandoVerifica.Parameters.AddWithValue("@Email", dto.Email);
                var count = (long?)await comandoVerifica.ExecuteScalarAsync();

                if (count == 0)
                    throw new Exception("Nenhum usuário encontrado com esse e-mail.");

                // Insere o token
                var comando = new NpgsqlCommand(
                    "INSERT INTO redefinicao_senha (email, token, validade) VALUES (@Email, @Token, @Validade)",
                    conexao
                );
                comando.Parameters.AddWithValue("@Email", dto.Email);
                comando.Parameters.AddWithValue("@Token", token);
                comando.Parameters.AddWithValue("@Validade", DateTime.Now.AddHours(1));
                await comando.ExecuteNonQueryAsync();
            }

            // Monta link e corpo do e-mail
            string baseUrl = _configuracao["FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:8080";
            string linkRedefinicao = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(token)}";

            string corpoEmail = $@"
                <p>Você solicitou redefinição de senha.</p>
                <p>Clique no link abaixo para alterar sua senha:</p>
                <p><a href='{linkRedefinicao}'>{linkRedefinicao}</a></p>
                <p>Se não foi você, ignore este e-mail.</p>
            ";

            // Envia o e-mail de forma assíncrona
            await _gmailServico.EnviarEmailAsync(dto.Email, "Redefinição de senha", corpoEmail);
        }

        public async Task<bool> ValidarToken(string token)
        {
            await using var conexao = new NpgsqlConnection(_stringConexao);
            await conexao.OpenAsync();

            var comando = new NpgsqlCommand("SELECT validade FROM redefinicao_senha WHERE token = @Token", conexao);
            comando.Parameters.AddWithValue("@Token", token);

            var validadeObj = await comando.ExecuteScalarAsync();
            if (validadeObj == null) return false;

            if (!DateTime.TryParse(validadeObj.ToString(), out DateTime validade))
                return false;

            return validade >= DateTime.Now;
        }

        public async Task AtualizarSenha(string token, string senha)
        {
            await using var conexao = new NpgsqlConnection(_stringConexao);
            await conexao.OpenAsync();

            var comandoBusca = new NpgsqlCommand(
                "SELECT email FROM redefinicao_senha WHERE token = @Token AND validade > @Agora",
                conexao
            );
            comandoBusca.Parameters.AddWithValue("@Token", token);
            comandoBusca.Parameters.AddWithValue("@Agora", DateTime.UtcNow);

            var emailObj = await comandoBusca.ExecuteScalarAsync();
            if (emailObj == null)
                throw new Exception("Token inválido ou expirado.");

            string email = emailObj.ToString()!;
            string senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);

            var comandoUpdate = new NpgsqlCommand("UPDATE usuarios SET senha = @Senha WHERE email = @Email", conexao);
            comandoUpdate.Parameters.AddWithValue("@Senha", senhaHash);
            comandoUpdate.Parameters.AddWithValue("@Email", email);
            await comandoUpdate.ExecuteNonQueryAsync();

            var comandoDelete = new NpgsqlCommand("DELETE FROM redefinicao_senha WHERE token = @Token", conexao);
            comandoDelete.Parameters.AddWithValue("@Token", token);
            await comandoDelete.ExecuteNonQueryAsync();
        }

        private static string GerarToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");
        }
    }
}
