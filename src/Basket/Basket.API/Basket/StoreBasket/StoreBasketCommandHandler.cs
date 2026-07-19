using Basket.API.Data;
using Basket.API.Models;
using BuildingBlocks.CQRS;
using FluentValidation;
using System.Windows.Input;

namespace Basket.API.Basket.StoreBasket
{
    public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;
    public record StoreBasketResult(string UserName);

    public class StoreBasketCommandValidator : AbstractValidator<StoreBasketCommand>
    {
        public StoreBasketCommandValidator()
        {
            RuleFor(x => x.Cart).NotNull().WithMessage("El carrito no puede ser nulo");
            RuleFor(x => x.Cart.UserName).NotEmpty().WithMessage("El nombre del usuario es requerido");
            RuleFor(x => x.Cart.Items).NotEmpty().WithMessage("El carrito no puede estar vacío");
        }
    }
    public class StoreBasketCommandHandler(IBasketRepository repository) : ICommandHandler<StoreBasketCommand, StoreBasketResult>
    {
        public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
        {
            ShoppingCart cart = command.Cart;
            await repository.StoreBasket(cart, cancellationToken);
            return new StoreBasketResult(command.Cart.UserName);
        }
    }
}
