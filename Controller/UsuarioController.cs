using LotoReg.Dtos;
using LotoReg.Interface;
using LotoReg.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LotoReg.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuario _usuarioService;

        public UsuarioController(IUsuario usuarioService)
        {
            _usuarioService = usuarioService;
        }

        
        [HttpPost("Cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] CadastroUsuarioDto dto)
        {
            try
            {
                await _usuarioService.CadastrarUsuario(dto);
                return Ok(new { mensagem = "Usuário cadastrado com sucesso!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

       
        [Authorize]
        [HttpGet("Buscar")]
        public async Task<IActionResult> Buscar()
        {
            try
            {
                var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario <= 0)
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });

                var usuario = await _usuarioService.GetUsuario(idUsuario)!;
                if (usuario == null)
                    return NotFound(new { mensagem = "Usuário não encontrado." });

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

       
        [Authorize]
        [HttpPut("Editar")]
        public async Task<IActionResult> Editar([FromBody] EditarUsuarioDto dto)
        {
            try
            {
                var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario <= 0)
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });

                await _usuarioService.EditarUsuario(idUsuario, dto);
                return Ok(new { mensagem = "Usuário atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            try
            {
                await _usuarioService.DeletarUsuario(id);
                return Ok(new { mensagem = "Usuário deletado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }
    }
}
