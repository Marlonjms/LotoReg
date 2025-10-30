namespace LotoReg.Interface
{
    public interface IVerificacaoEmail
    {
        Task EnviarCodigoVerificacao(string email);
        Task <bool> ConfirmarCodigo(string email, string codigo);
        Task <bool> EstaConfirmado(string email);
    }
}
