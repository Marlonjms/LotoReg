using LotoReg.Interface;
using LotoReg.Models;
using Npgsql;

namespace LotoReg.Service
{
    public class VisaoGeralService : IVisaoGeral
    {
        private readonly string _stringConexao;

        public VisaoGeralService(IConfiguration configuracao)
        {
            _stringConexao = configuracao.GetConnectionString("DefaultConnection")!;
        }

        public async Task<ModeloResposta> VisaoGeral(int idUsuario)
        {
            await using var conexao = new NpgsqlConnection(_stringConexao);
            await conexao.OpenAsync();

            const string query = @"
                SELECT 
                    COUNT(*) AS total,
                    COUNT(CASE WHEN status = 'RequerimentoEmAnalise' THEN 1 END) AS em_analise,
                    COUNT(CASE WHEN status = 'RequerimentoAprovado' THEN 1 END) AS aprovados
                FROM requerimentos
                WHERE usuario_id = @IdUsuario;
            ";

            await using var cmd = new NpgsqlCommand(query, conexao);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ModeloResposta
                {
                    TodosRequerimentos = reader.GetInt32(reader.GetOrdinal("total")),
                    RequerimentosEmAnalise = reader.GetInt32(reader.GetOrdinal("em_analise")),
                    RequerimentosAprovados = reader.GetInt32(reader.GetOrdinal("aprovados"))
                };
            }

            return new ModeloResposta();
        }
    }
}
