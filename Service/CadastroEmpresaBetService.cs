using LotoReg.Dtos;
using LotoReg.Interface;
using LotoReg.Models;
using Npgsql;

namespace LotoReg.Service
{
    public class CadastroEmpresaBetService : ICadastroEmpresaBet
    {
        private readonly string _stringConexao;

        public CadastroEmpresaBetService(IConfiguration configuracao)
        {
            _stringConexao = configuracao.GetConnectionString("DefaultConnection")!;
        }

        public async Task AtualizarEmpresa(int usuarioId, AtualizarEmpresaBetDto dto)
        {
            try
            {
                await using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

        
                const string queryVerificar = "SELECT id FROM empresa WHERE id_usuario = @UsuarioId AND categoria = 'EmpresaBet';";
                int? empresaId = null;

                await using (var cmdVerificar = new NpgsqlCommand(queryVerificar, conexao))
                {
                    cmdVerificar.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    var resultado = await cmdVerificar.ExecuteScalarAsync();
                    if (resultado != null)
                        empresaId = Convert.ToInt32(resultado);
                }

                if (empresaId == null)
                    throw new KeyNotFoundException("Empresa não encontrada para este usuário.");

                
                const string query = @"
                UPDATE empresa SET 
                razao_social = COALESCE(@RazaoSocial, razao_social),
                estado = COALESCE(@Estado, estado),
                endereco_completo = COALESCE(@EnderecoCompleto, endereco_completo),
                telefone_comercial = COALESCE(@TelefoneComercial, telefone_comercial),
                email_contato = COALESCE(@EmailContato, email_contato),
                site_plataforma = COALESCE(@SitePlataforma, site_plataforma)
                WHERE id_usuario = @UsuarioId AND categoria = 'EmpresaBet';";

                await using var cmd = new NpgsqlCommand(query, conexao);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@RazaoSocial", (object?)dto.RazaoSocial ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Estado", (object?)dto.Estado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EnderecoCompleto", (object?)dto.EnderecoCompleto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TelefoneComercial", (object?)dto.TelefoneComercial ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmailContato", (object?)dto.EmailContato ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SitePlataforma", (object?)dto.SitePlataforma ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {

                throw new Exception($"Erro ao atualizar empresa: {ex.Message}", ex);
            }
        }


        public async Task CadastrarEmpresa(CadastroEmpresaBetDto empresa, byte[] contratoSocialPdf, int idUsuario)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(empresa.RazaoSocial) ||
                    string.IsNullOrWhiteSpace(empresa.CNPJ) ||
                    string.IsNullOrWhiteSpace(empresa.Estado) ||
                    string.IsNullOrWhiteSpace(empresa.EnderecoCompleto) ||
                    string.IsNullOrWhiteSpace(empresa.TelefoneComercial) ||
                    string.IsNullOrWhiteSpace(empresa.EmailContato) ||
                    contratoSocialPdf == null || contratoSocialPdf.Length == 0)
                {
                    throw new ArgumentException("Todos os campos obrigatórios devem ser preenchidos.");
                }

                await using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();
                await using var transacao = await conexao.BeginTransactionAsync();

                try
                {

                    var queryVerificar = @"
                    SELECT COUNT(*) FROM empresa WHERE id_usuario = @Id_usuario AND categoria = 'EmpresaBet'; ";

                    await using (var cmdVerificar = new NpgsqlCommand(queryVerificar, conexao, transacao))
                    {
                        cmdVerificar.Parameters.AddWithValue("@Id_usuario", idUsuario);
                        var count = Convert.ToInt32(await cmdVerificar.ExecuteScalarAsync());

                        if (count > 0)
                            throw new InvalidOperationException("Usuário já possui uma empresa cadastrada na categoria 'EmpresaBet'.");
                    }


                    var queryEmpresa = @"
                     INSERT INTO empresa
                     (id_usuario, razao_social, cnpj, data_fundacao, estado, endereco_completo, telefone_comercial, 
                     email_contato, contrato_social_pdf, site_plataforma, categoria)
                     VALUES
                     (@Id_usuario, @RazaoSocial, @CNPJ, @DataFundacao, @Estado, @EnderecoCompleto, 
                     @TelefoneComercial, @EmailContato, @ContratoSocialPdf, @SitePlataforma, 'EmpresaBet')
                     RETURNING id;";

                    int empresaId;
                    await using (var cmd = new NpgsqlCommand(queryEmpresa, conexao, transacao))
                    {
                        cmd.Parameters.AddWithValue("@Id_usuario", idUsuario);
                        cmd.Parameters.AddWithValue("@RazaoSocial", empresa.RazaoSocial);
                        cmd.Parameters.AddWithValue("@CNPJ", empresa.CNPJ);
                        cmd.Parameters.AddWithValue("@DataFundacao", empresa.DataFundacao);
                        cmd.Parameters.AddWithValue("@Estado", empresa.Estado);
                        cmd.Parameters.AddWithValue("@EnderecoCompleto", empresa.EnderecoCompleto);
                        cmd.Parameters.AddWithValue("@TelefoneComercial", empresa.TelefoneComercial);
                        cmd.Parameters.AddWithValue("@EmailContato", empresa.EmailContato);
                        cmd.Parameters.Add("@ContratoSocialPdf", NpgsqlTypes.NpgsqlDbType.Bytea).Value = contratoSocialPdf;
                        cmd.Parameters.AddWithValue("@SitePlataforma", (object?)empresa.SitePlataforma ?? DBNull.Value);

                        empresaId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }


                    if (empresa.Socios != null && empresa.Socios.Any())
                    {
                        foreach (var socio in empresa.Socios)
                        {
                            var querySocio = @" INSERT INTO socios_empresa (empresa_id, nome) VALUES (@EmpresaId, @Nome); ";

                            await using var cmdSocio = new NpgsqlCommand(querySocio, conexao, transacao);
                            cmdSocio.Parameters.AddWithValue("@EmpresaId", empresaId);
                            cmdSocio.Parameters.AddWithValue("@Nome", socio.Nome);

                            await cmdSocio.ExecuteNonQueryAsync();
                        }
                    }

                    await transacao.CommitAsync();
                }
                catch
                {
                    await transacao.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao cadastrar empresa: {ex.Message}", ex);
            }
        }


        public async Task<BuscarEmpresaBet?> ObterEmpresa(int idUsuario)
        {
            try
            {
                await using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

                // Busca a empresa do usuário
                const string queryEmpresa = @"
                SELECT * FROM empresa WHERE id_usuario = @IdUsuario AND categoria = 'EmpresaBet'; ";

                await using var cmdEmpresa = new NpgsqlCommand(queryEmpresa, conexao);
                cmdEmpresa.Parameters.AddWithValue("@IdUsuario", idUsuario);

                BuscarEmpresaBet? empresa = null;

                await using (var reader = await cmdEmpresa.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        empresa = new BuscarEmpresaBet
                        {
                            RazaoSocial = reader["razao_social"].ToString()!,
                            CNPJ = reader["cnpj"].ToString()!,
                            Estado = reader["estado"].ToString()!,
                            EnderecoCompleto = reader["endereco_completo"].ToString()!,
                            TelefoneComercial = reader["telefone_comercial"].ToString()!,
                            EmailContato = reader["email_contato"].ToString()!,
                            SitePlataforma = reader["site_plataforma"].ToString(),
                            DataFundacao = Convert.ToDateTime(reader["data_fundacao"]),
                            Socios = new List<SociosDto>(),
                            ContratoSocialPdf = reader["contrato_social_pdf"] == DBNull.Value
                                ? null
                                : (byte[])reader["contrato_social_pdf"]
                        };
                    }
                }

                if (empresa == null)
                    return null;

                
                const string querySocios = @"
                SELECT nome FROM socios_empresa WHERE empresa_id = ( SELECT id FROM empresa  
                WHERE id_usuario = @IdUsuario AND categoria = 'EmpresaBet' ); ";

                await using var cmdSocios = new NpgsqlCommand(querySocios, conexao);
                cmdSocios.Parameters.AddWithValue("@IdUsuario", idUsuario);

                await using var readerSocios = await cmdSocios.ExecuteReaderAsync();
                while (await readerSocios.ReadAsync())
                {
                    empresa.Socios!.Add(new SociosDto
                    {
                        Nome = readerSocios["nome"].ToString()!
                    });
                }

                return empresa;
            }
           
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter empresa: {ex.Message}", ex);
            }
        }

    }
}
