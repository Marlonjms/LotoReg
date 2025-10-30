using LotoReg.Interface;
using LotoReg.Service;
using Helpers;
using LotoReg.Serviços;

var builder = WebApplication.CreateBuilder(args);

// Registra o CORS (antes do Build)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        //policy.WithOrigins("https://incandescent-sherbet-54d32d.netlify.app")
        //    .AllowAnyHeader()
        //    .AllowAnyMethod();
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

    });
});

// JWT e Swagger
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IUsuario, UsuarioService>();
builder.Services.AddScoped<IAutenticacao, AutenticacaoServiço>();
builder.Services.AddScoped<IRedefinicaoSenha, RedefinicaoSenhaServico>();
builder.Services.AddScoped<IVerificacaoEmail, VerificacaoEmailServico>();
builder.Services.AddScoped<ICadastroEmpresaBet, CadastroEmpresaBetService>();
builder.Services.AddScoped<ICadastroEmpresaSorteio, CadastroEmpresaSorteioService>();

builder.Services.AddSingleton<GmailServico>();

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseCors("AllowReactApp");  // <-- middleware do CORS ativado aqui!

app.UseAuthorization();

app.MapControllers();

app.Run();
