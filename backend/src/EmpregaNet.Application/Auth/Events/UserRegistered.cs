using EmpregaNet.Application.Abstraction;

namespace EmpregaNet.Application.Auth.Events;

/// <summary>
/// Conta criada por registo próprio. Publicado depois do commit pelo
/// <c>NotificationDispatchBehavior</c>.
/// </summary>
/// <remarks>
/// Transporta apenas o identificador da conta. O consumidor resolve o utilizador a partir de
/// <paramref name="UserId"/>, evitando confiar em dados da requisição HTTP. Assim, o destinatário
/// continua derivado do registo persistido, tal como em <c>JobApplicationStatusChanged</c>.
///
/// <para>
/// O envio do e-mail não acontece no comando porque <c>RegisterUserCommand</c> é
/// <c>ITransactional</c>. Executar esse envio dentro da transação cria falhas que o rollback não
/// corrige: um commit inválido pode deixar um link de confirmação enviado para um registo
/// inexistente, e uma reexecução por retry pode enviar um segundo e-mail..
/// </para>
/// </remarks>
/// <param name="UserId">Identificador da conta criada; é para esta conta que o link de confirmação é enviado.</param>
public sealed record UserRegistered(long UserId) : IIdentifiableNotification
{
    public string DeduplicationKey => UserId.ToString();
}
