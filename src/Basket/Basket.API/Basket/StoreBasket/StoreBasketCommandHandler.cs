using Basket.API.Data;
using Basket.API.Models;
using BuildingBlocks.CQRS;
using FluentValidation;

namespace Basket.API.Basket.StoreBasket
{
    public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;
    public record StoreBasketResult(string UserName);

    public class StoreBasketCommandValidator : AbstractValidator<StoreBasketCommand>
    {
        public StoreBasketCommandValidator()
        {
            RuleFor(x => x.Cart)
                .NotNull()
                .WithMessage("El carrito no puede ser nulo");

            RuleFor(x => x.Cart.UserName)
                .NotEmpty()
                .WithMessage("El nombre del usuario es requerido");

            RuleFor(x => x.Cart.Items)
                .NotEmpty()
                .WithMessage("El carrito no puede estar vacío");
        }
    }

    public class StoreBasketCommandHandler(IBasketRepository repository) : ICommandHandler<StoreBasketCommand, StoreBasketResult>
    {
        public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
        {
            // Protección de seguridad para evitar que llegue un ID vacío o nulo a Marten/PostgreSQL
            ArgumentNullException.ThrowIfNull(command.Cart);
            if (string.IsNullOrWhiteSpace(command.Cart.UserName))
            {
                throw new ArgumentException("El nombre del usuario es requerido para guardar el carrito.");
            }

            ShoppingCart cart = command.Cart;
            await repository.StoreBasket(cart, cancellationToken);
            return new StoreBasketResult(command.Cart.UserName);
        }
    }
}