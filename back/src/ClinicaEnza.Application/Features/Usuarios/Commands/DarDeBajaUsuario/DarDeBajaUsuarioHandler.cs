using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;

namespace ClinicaEnza.Application.Features.Usuarios.Commands.DarDeBajaUsuario;

public class DarDeBajaUsuarioHandler : IRequestHandler<DarDeBajaUsuarioCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DarDeBajaUsuarioHandler(IUnitOfWork unitOfWork)
        : this(unitOfWork?.Usuarios ?? throw new ArgumentNullException(nameof(unitOfWork)), unitOfWork)
    {
    }

    public DarDeBajaUsuarioHandler(IUsuarioRepository usuarioRepository, IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result> Handle(DarDeBajaUsuarioCommand request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var usuario = await _usuarioRepository.GetByIdAsync(request.IdUsuario, cancellationToken);
        if (usuario == null)
            return Result.Failure($"No se encontró el usuario con ID {request.IdUsuario}.");

        try
        {
            usuario.DarDeBaja();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class DarDeBajaUsuarioCommandHandler : DarDeBajaUsuarioHandler
{
    public DarDeBajaUsuarioCommandHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
    }

    public DarDeBajaUsuarioCommandHandler(IUsuarioRepository usuarioRepository, IUnitOfWork unitOfWork)
        : base(usuarioRepository, unitOfWork)
    {
    }
}

