using LotoReg.Dtos;
using LotoReg.Interface;
using Npgsql;

namespace LotoReg.Service
{
    public class RequerimentoSorteioService : IRequerimentoSorteio
    {
        private readonly string _stringConexao;

        public RequerimentoSorteioService(IConfiguration configuracao)
        {
            _stringConexao = configuracao.GetConnectionString("DefaultConnection")!;
        }


        public async Task CadastrarModeloRequerimento(ModeloRequerimentoDto dto)
        {
            try
            {
                
                if (dto == null)
                    throw new Exception("Os dados do modelo são obrigatórios.");

                if (dto.Arquivo == null || dto.Arquivo.Length == 0)
                    throw new Exception("O arquivo PDF do modelo é obrigatório.");

                if (string.IsNullOrWhiteSpace(dto.Nome) && string.IsNullOrEmpty(dto.Arquivo?.FileName))
                    throw new Exception("O nome do modelo é obrigatório.");

                await using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

                using var ms = new MemoryStream();
                await dto.Arquivo.CopyToAsync(ms);
                var bytes = ms.ToArray();


                var nome = string.IsNullOrWhiteSpace(dto.Nome)
                    ? dto.Arquivo.FileName
                    : dto.Nome;

                var query = @"
                              INSERT INTO modelos_requerimento (nome, arquivo, categoria)
                              VALUES (@nome, @arquivo, 'RequerimentoSorteio');
                            ";

                await using var cmd = new NpgsqlCommand(query, conexao);
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@arquivo", bytes);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {

                throw new Exception($"Erro ao cadastrar o modelo de requerimento: {ex.Message}");
            }
        }


        public async Task<List<ModeloRequerimentoDownloadDto>> ObterTodosModelos()
        {
            try
            {
                var lista = new List<ModeloRequerimentoDownloadDto>();

                await using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

                var query = "SELECT id, nome FROM modelos_requerimento WHERE categoria = 'RequerimentoSorteio';";
                await using var cmd = new NpgsqlCommand(query, conexao);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    lista.Add(new ModeloRequerimentoDownloadDto
                    {
                        Id = reader.GetInt32(0),
                        Nome = reader.GetString(1)
                    });
                }

                if (!lista.Any())
                    throw new Exception("Nenhum modelo de requerimento encontrado.");

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter modelos: {ex.Message}");
            }
        }




        public async Task<BaixarModeloRequerimentoDto> BaixarModeloPorId(int id)
        {
            try
            {
                await using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

                var query = "SELECT nome, arquivo FROM modelos_requerimento WHERE id = @id AND categoria = 'RequerimentoSorteio';";
                await using var cmd = new NpgsqlCommand(query, conexao);
                cmd.Parameters.AddWithValue("@id", id);

                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new BaixarModeloRequerimentoDto
                    {
                        Nome = reader.GetString(0),
                        Arquivo = (byte[])reader["arquivo"]
                    };
                }

                throw new Exception("Modelo não encontrado.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao baixar modelo: {ex.Message}");
            }
        }




        public async Task EnviarRequerimento(EnviarRequerimentoDto dto, int usuarioId)
        {
            try
            {
                if (dto == null)
                    throw new Exception("Os dados do requerimento são obrigatórios.");

                if (string.IsNullOrWhiteSpace(dto.Tipo))
                    throw new Exception("O campo 'Tipo' é obrigatório.");

                if (dto.Documento == null || dto.Documento.Length == 0)
                    throw new Exception("O arquivo do requerimento é obrigatório.");

               
                var protocolo = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            
                var fuso = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                var dataHoraBrasil = TimeZoneInfo.ConvertTime(DateTime.UtcNow, fuso);

                await using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

                using var ms = new MemoryStream();
                await dto.Documento.CopyToAsync(ms);
                var arquivoBytes = ms.ToArray();

                var nomeArquivo = dto.Documento.FileName;

                var query = @"
                    INSERT INTO requerimentos 
                    (usuario_id, protocolo, tipo, observacoes, arquivo, nome_arquivo, data_hora, status, categoria)
                    VALUES (@usuario_id, @protocolo, @tipo, @observacoes, @arquivo, @nome_arquivo, @data_hora, @status, 'RequerimentoSorteio');
                ";

                await using var cmd = new NpgsqlCommand(query, conexao);
                cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
                cmd.Parameters.AddWithValue("@protocolo", protocolo);
                cmd.Parameters.AddWithValue("@tipo", dto.Tipo);
                cmd.Parameters.AddWithValue("@observacoes", (object?)dto.Observacoes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@arquivo", arquivoBytes);
                cmd.Parameters.AddWithValue("@nome_arquivo", nomeArquivo);
                cmd.Parameters.AddWithValue("@data_hora", dataHoraBrasil);
                cmd.Parameters.AddWithValue("@status", "Requerimento Protocolado");

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao enviar requerimento: {ex.Message}");
            }
        }



        public async Task<List<RequerimentoResumoDto>> ObterTodosRequerimentos(int usuarioId)
        {
            try
            {
                var lista = new List<RequerimentoResumoDto>();

                await using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

                var query = @"
                SELECT id, protocolo, tipo, observacoes, data_hora, status, nome_arquivo 
                FROM requerimentos
                WHERE usuario_id = @usuario_id AND categoria = 'RequerimentoSorteio'
                ORDER BY data_hora DESC";

                await using var cmd = new NpgsqlCommand(query, conexao);
                cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    lista.Add(new RequerimentoResumoDto
                    {
                        Id = reader.GetInt32(0),
                        Protocolo = reader.GetString(1),
                        Tipo = reader.GetString(2),
                        Observacoes = reader.IsDBNull(3) ? null : reader.GetString(3),
                        DataHora = reader.GetDateTime(4),
                        Status = reader.GetString(5),
                        NomeArquivo = reader.IsDBNull(6) ? null : reader.GetString(6) 
                    });
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter requerimentos: {ex.Message}");
            }
        }


        public async Task DeletarModeloPorId(int id)
        {
            try
            {
                await using var conexao = new NpgsqlConnection(_stringConexao);
                await conexao.OpenAsync();

                var verificarQuery = "SELECT COUNT(*) FROM modelos_requerimento WHERE id = @id AND categoria = 'RequerimentoSorteio';";
                await using var verificarCmd = new NpgsqlCommand(verificarQuery, conexao);
                verificarCmd.Parameters.AddWithValue("@id", id);

                var result = await verificarCmd.ExecuteScalarAsync();
                var count = Convert.ToInt64(result ?? 0);

                if (count == 0)
                    throw new Exception("Modelo não encontrado para exclusão.");

                var query = "DELETE FROM modelos_requerimento WHERE id = @id";
                await using var cmd = new NpgsqlCommand(query, conexao);
                cmd.Parameters.AddWithValue("@id", id);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao deletar modelo: {ex.Message}");
            }
        }

    }
}
