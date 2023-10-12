﻿using BurgerRoyale.Application.DTO;
using BurgerRoyale.Application.Interface.Services;
using BurgerRoyale.Application.Models;
using BurgerRoyale.Application.Services;
using BurgerRoyale.Domain.Entities;
using BurgerRoyale.Domain.Interface.Repositories;
using Moq;

namespace BurgerRoyale.UnitTests.Application
{
    public class ProductServiceShould
    {
        private readonly Mock<IProductRepository> productRepositoryMock;
        
        private readonly IProductService productService;

        public ProductServiceShould()
        {
            productRepositoryMock = new Mock<IProductRepository>();

            productService = new ProductService(productRepositoryMock.Object);
        }

        [Fact]
        public async Task Add_New_Product()
        {
            #region Arrange(Given)

            string name = "Bacon burger";
            Guid categoryId = Guid.NewGuid();
            string description = "Delicious bacon burger";
            decimal price = 20;

            ProductDTO addProductRequestDTO = new()
            {
                Name = name,
                CategoryId = categoryId,
                Description = description,
                Price = price,
            };

            #endregion

            #region Act(When)

            AddProductResponse response = await productService.AddAsync(addProductRequestDTO);

            #endregion

            #region Assert(Then)

            Assert.NotNull(response);
            Assert.True(response.IsValid);

            productRepositoryMock
                .Verify(
                    repository => repository.AddAsync(It.Is<Product>(product => 
                        product.Name == name &&
                        product.CategoryId == categoryId &&
                        product.Description == description &&
                        product.Price == price)),
                    Times.Once());

            #endregion
        }

        [Fact]
        public async Task Return_Notification_When_Request_Is_Invalid()
        {
            #region Arrange(Given)

            string name = string.Empty;
            Guid categoryId = Guid.Empty;
            string description = "";
            decimal price = 0;

            ProductDTO addProductRequestDTO = new()
            {
                Name = name,
                CategoryId = categoryId,
                Description = description,
                Price = price,
            };

            #endregion

            #region Act(When)

            AddProductResponse response = await productService.AddAsync(addProductRequestDTO);

            #endregion

            #region Assert(Then)

            Assert.False(response.IsValid);
            Assert.True(response.Notifications.Any());

            productRepositoryMock
                .Verify(repository => repository.AddAsync(It.IsAny<Product>()), 
                Times.Never());

            #endregion
        }

        [Fact]
        public async Task Get_By_Id()
        {
            #region Arrange(Given)

            Guid productId = Guid.NewGuid();

            var product = new Product("Bacon burger", "", 100, Guid.NewGuid());

            productRepositoryMock
                .Setup(repository => repository.GetByIdAsync(productId))
                .ReturnsAsync(product);

            #endregion

            #region Act(When)

            GetProductResponse response = await productService.GetById(productId);

            #endregion

            #region Assert(Then)

            Assert.NotNull(response);
            Assert.True(response.IsValid);
            Assert.NotNull(response.Product);
 
            Assert.Equal(product.Name, response.Product.Name);
            Assert.Equal(product.Price, response.Product.Price);
            Assert.Equal(product.CategoryId, response.Product.CategoryId);
            
            #endregion
        }

        [Fact]
        public async Task Return_Notification_When_Product_Does_Not_Exist()
        {
            #region Arrange(Given)

            Guid productId = Guid.NewGuid();

            productRepositoryMock
                .Setup(repository => repository.GetByIdAsync(productId))
                .ReturnsAsync(() => null);

            #endregion

            #region Act(When)

            GetProductResponse response = await productService.GetById(productId);

            #endregion

            #region Assert(Then)

            Assert.False(response.IsValid);
            Assert.Equal("The product does not exist", response.Notifications.First().Message);

            #endregion
        }
    }
}