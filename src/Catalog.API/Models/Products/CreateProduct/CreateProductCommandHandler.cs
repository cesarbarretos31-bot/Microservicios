using BuildingBlocks.CQRS;

namespace Catalog.API.Models.Products.CreateProduct
{
    //record nos permite crear el producto con los datos para registrar como uno nuevo
    public record CreateProductCommandHandler(string Name, string Description, List<string> Category, string ImageFiles, decimal Price) : ICommand<CreateProductResult>;

    //este record retorna el objeto de respuesta, es decir, el identificador del objeto insertado
    public record CreateProductResult(Guid Id);

    internal class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
    {
        public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            //Aquí la lógica para crear el producto y retornar el resultado 
            Product product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                ImageFiles = request.ImageFiles,
                Price = request.Price
            };

            // Salvar a base de datos <- Poner otro patron de diseño que se encargue de esa accion
            return new CreateProductResult(Guid.NewGuid());

        }
    }
}
