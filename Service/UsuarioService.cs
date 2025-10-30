using LotoReg.Dtos;
using LotoReg.Interface;
using LotoReg.Models;
using Npgsql;

namespace LotoReg.Service
{
    public class UsuarioService : IUsuario
    {
        private readonly string _stringConexao;
        private readonly IVerificacaoEmail _verificacaoEmail;

        public UsuarioService(IConfiguration configuration, IVerificacaoEmail verificacaoEmail)
        {
            _stringConexao = configuration.GetConnectionString("DefaultConnection")!;
            _verificacaoEmail = verificacaoEmail;
        }

        public async Task CadastrarUsuario(CadastroUsuarioDto cadastroUsuarioDto)
        {
            try
            {
                using var conexao = new NpgsqlConnection(_stringConexao);

                if (string.IsNullOrWhiteSpace(cadastroUsuarioDto.RazaoSocial) ||
                    string.IsNullOrWhiteSpace(cadastroUsuarioDto.Email) ||
                    string.IsNullOrWhiteSpace(cadastroUsuarioDto.Senha))
                {
                    throw new ArgumentException("Todos os campos são obrigatórios.");
                }

                // Normaliza o e-mail
                string emailNormalizado = cadastroUsuarioDto.Email.Trim().ToLower();

                // Verifica se o e-mail foi confirmado
                if (!await _verificacaoEmail.EstaConfirmado(emailNormalizado))
                {
                    throw new Exception("E-mail ainda não foi confirmado. Verifique sua caixa de entrada.");
                }

                await conexao.OpenAsync();

                // Verifica se já existe o e-mail
                var comandoVerificacao = new NpgsqlCommand("SELECT COUNT(*) FROM usuarios WHERE email = @Email", conexao);
                comandoVerificacao.Parameters.AddWithValue("@Email", emailNormalizado);
                int count = Convert.ToInt32(await comandoVerificacao.ExecuteScalarAsync());

                if (count > 0)
                    throw new ArgumentException("Já existe um usuário cadastrado com este e-mail.");

                // Gera o hash da senha
                string senhaHash = BCrypt.Net.BCrypt.HashPassword(cadastroUsuarioDto.Senha);

                // Insere o novo usuário
                string query = @"INSERT INTO usuarios (razao_social, senha, email)
                                 VALUES (@razao_social, @senha, @Email)";

                using var cmd = new NpgsqlCommand(query, conexao);
                cmd.Parameters.AddWithValue("@razao_social", cadastroUsuarioDto.RazaoSocial);
                cmd.Parameters.AddWithValue("@senha", senhaHash);
                cmd.Parameters.AddWithValue("@Email", emailNormalizado);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao cadastrar usuário: " + ex.Message);
            }
        }

        public async Task<BuscarUsuarioModelo?> GetUsuario(int idUsuario)
        {
            try
            {
                using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

                string query = @"SELECT id, razao_social, email
                                 FROM usuarios
                                 WHERE id = @id";

                using var cmd = new NpgsqlCommand(query, conexao);
                cmd.Parameters.AddWithValue("@id", idUsuario);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new BuscarUsuarioModelo
                    {
                        Id = reader.GetInt32(0),
                        RazaoSocial = reader.GetString(1),
                        Email = reader.GetString(2)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar usuário: " + ex.Message);
            }
        }

        public async Task EditarUsuario(int id, EditarUsuarioDto dto)
        {
            try
            {
                using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

                string query = @"UPDATE usuarios
                                 SET razao_social = @razao_social
                                 WHERE id = @id";

                using var cmd = new NpgsqlCommand(query, conexao);
                cmd.Parameters.AddWithValue("@razao_social", dto.RazaoSocial);
                cmd.Parameters.AddWithValue("@id", id);

                int linhasAfetadas = await cmd.ExecuteNonQueryAsync();

                if (linhasAfetadas == 0)
                {
                    throw new Exception("Usuário não encontrado.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao editar usuário: " + ex.Message);
            }
        }

        public async Task DeletarUsuario(int idUsuario)
        {
            try
            {
                using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

                string query = @"DELETE FROM usuarios WHERE id = @id";

                using var cmd = new NpgsqlCommand(query, conexao);
                cmd.Parameters.AddWithValue("@id", idUsuario);

                int linhasAfetadas = await cmd.ExecuteNonQueryAsync();

                if (linhasAfetadas == 0)
                {
                    throw new Exception("Usuário não encontrado.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao deletar usuário: " + ex.Message);
            }
        }
    }
}
