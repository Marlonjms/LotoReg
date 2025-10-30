using Microsoft.AspNetCore.Mvc;
using LotoReg.Interface;
using LotoReg.Dtos;


namespace LotoReg.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedefinicaoSenhaController : ControllerBase
    {
        private readonly IRedefinicaoSenha _servicoRedefinicao;

        public RedefinicaoSenhaController(IRedefinicaoSenha servicoRedefinicao)
        {
            _servicoRedefinicao = servicoRedefinicao;
        }

        [HttpPost("solicitar")]
        public async Task<IActionResult> SolicitarRedefinicao([FromBody] EditarSenhaRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email))
                return BadRequest(new { mensagem = "Email é obrigatório" });

            try
            {
                await _servicoRedefinicao.EnviarEmailRedefinicao(dto);
                return Ok(new { mensagem = "Email enviado com instruções para redefinir a senha." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("validar-token")]
        public async Task<IActionResult> ValidarToken([FromQuery] string token)
        {
            bool valido = await _servicoRedefinicao.ValidarToken(token);
            if (!valido)
                return BadRequest(new { mensagem = "Token inválido ou expirado." });

            return Ok(new { mensagem = "Token válido." });
        }

        [HttpPost("atualizar-senha")]
        public async Task<IActionResult> AtualizarSenha([FromBody] AtualizarSenhaRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.NovaSenha))
                return BadRequest(new { mensagem = "Nova senha é obrigatória." });

            try
            {
                await _servicoRedefinicao.AtualizarSenha(dto.Token, dto.NovaSenha);
                return Ok(new { mensagem = "Senha atualizada com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}
