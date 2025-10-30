using Microsoft.AspNetCore.Mvc;
using LotoReg.Dtos;
using LotoReg.Interface;

namespace LotoReg.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly IAutenticacao _servicoAutenticacao;

        public AutenticacaoController(IAutenticacao servicoAutenticacao)
        {
            _servicoAutenticacao = servicoAutenticacao;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUsuarioDto loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Senha))
            {
                return BadRequest(new { mensagem = "Email e senha são obrigatórios." });
            }

            try
            {
                var token = await _servicoAutenticacao.Logar(loginDto); // 👈 AQUI entra o await
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

    }
}
