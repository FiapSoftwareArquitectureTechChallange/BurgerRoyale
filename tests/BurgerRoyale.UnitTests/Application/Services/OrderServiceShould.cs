﻿using BurgerRoyale.Application.Services;
using BurgerRoyale.Domain.DTO;
using BurgerRoyale.Domain.Entities;
using BurgerRoyale.Domain.Enumerators;
using BurgerRoyale.Domain.Helpers;
using BurgerRoyale.Domain.Interface.Repositories;
using BurgerRoyale.Domain.Interface.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace BurgerRoyale.UnitTests.Application.Services;

public class OrderServiceShould
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;

    private readonly IOrderService _orderService;

    public OrderServiceShould()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _httpContextAccessor = new Mock<IHttpContextAccessor>();

        _orderService = new OrderService(
            _orderRepositoryMock.Object, 
            _productRepositoryMock.Object,
            _httpContextAccessor.Object
        );
    }

    [Fact]
    public async Task Create_New_Order()
    {
        #region Arrange(Given)

        // Usuário
        var userId = Guid.NewGuid();

        //Produto
        var productId = Guid.NewGuid();
        var product = new Product("Burger", "Big burger", 20, ProductCategory.Lanche);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        CreateOrderProductDTO orderProduct = new CreateOrderProductDTO()
        {
            ProductId = productId,
            Quantity = 1
        };

        var orderProducts = new List<CreateOrderProductDTO>
        {
            orderProduct
        };

        //Pedido
        CreateOrderDTO orderDTO = new()
        {
            OrderProducts = orderProducts
        };

        #endregion Arrange(Given)

        #region Act(When)

        var exception = await Record.ExceptionAsync(async () => await _orderService.CreateAsync(orderDTO));

        #endregion Act(When)

        #region Assert(Then)

        Assert.Null(exception);

        _orderRepositoryMock
            .Verify(
                repository => repository.AddAsync(It.Is<Order>(order =>
                    order.OrderProducts.Count() == 1)),
                Times.Once());

        #endregion Assert(Then)
    }

    [Fact]
    public async Task CreateOrder_WithInvalidProduct_ThenShouldGiveAnException()
    {
        #region Arrange(Given)

        // Usuário
        var userId = Guid.NewGuid();

        //Produto
        var productId = Guid.NewGuid();
        var product = new Product("Burger", "Big burger", 20, ProductCategory.Lanche);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        CreateOrderProductDTO orderProduct = new CreateOrderProductDTO()
        {
            ProductId = Guid.NewGuid(),
            Quantity = 1
        };

        var orderProducts = new List<CreateOrderProductDTO>
        {
            orderProduct
        };

        //Pedido
        CreateOrderDTO orderDTO = new()
        {
            OrderProducts = orderProducts
        };

        #endregion Arrange(Given)

        #region Act(When)

        var exception = await Record.ExceptionAsync(async () => await _orderService.CreateAsync(orderDTO));

        #endregion Act(When)

        #region Assert(Then)

        Assert.NotNull(exception);
        Assert.Equal("Produto(s) inválido(s).", exception.Message);

        #endregion Assert(Then)
    }

    [Fact]
    public async Task Get_Orders()
    {
        #region Arrange(Given)

        // Produto
        var productId = Guid.NewGuid();
        var productName = "Test";
        var productDesc = "Test description";
        var orderId = Guid.NewGuid();
        decimal productPrice = 30;
        int quantity = 1;
        var productCategory = ProductCategory.Sobremesa;

        var product = new Product(productName, productDesc, productPrice, productCategory);

        // Order product
        OrderProduct orderProduct = new OrderProduct(orderId, productId, productPrice, quantity, product);

        var userId = Guid.NewGuid();
        var orderStatus = OrderStatus.EmPreparacao;

        //Pedido
        Order order = new(userId);
        order.AddProduct(orderProduct);
        order.SetStatus(orderStatus);

        var orderList = new List<Order>
        {
            order
        };

        _orderRepositoryMock
            .Setup(x => x.GetOrders(null, null))
            .ReturnsAsync(orderList);

        #endregion Arrange(Given)

        #region Act(When)

        var orders = await _orderService.GetOrdersAsync(null);

        #endregion Act(When)

        #region Assert(Then)

        _orderRepositoryMock
            .Verify(
                repository => repository.GetOrders(null, null),
                Times.Once());

        Assert.NotNull(orders);
        Assert.Single(orders);
        Assert.Equal(30, orders.FirstOrDefault().TotalPrice);
        Assert.Equal(OrderStatus.EmPreparacao.GetDescription(), orders.FirstOrDefault().Status);

        #endregion Assert(Then)
    }

    [Fact]
    public async Task Remove_Order()
    {
        #region Arrange(Given)

        // Produto
        var productId = Guid.NewGuid();
        var productName = "Test";
        var productDesc = "Test description";
        var orderId = Guid.NewGuid();
        decimal productPrice = 30;
        int quantity = 1;
        var productCategory = ProductCategory.Sobremesa;

        var product = new Product(productName, productDesc, productPrice, productCategory);

        // Order product
        OrderProduct orderProduct = new OrderProduct(orderId, productId, productPrice, quantity, product);

        var userId = Guid.NewGuid();
        var orderStatus = OrderStatus.EmPreparacao;

        //Pedido
        Order order = new(userId);
        order.AddProduct(orderProduct);
        order.SetStatus(orderStatus);

        var orderList = new List<Order>
        {
            order
        };

        _orderRepositoryMock
            .Setup(x => x.GetOrders(null, null))
            .ReturnsAsync(orderList);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        #endregion Arrange(Given)

        #region Act(When)

        var exception = await Record.ExceptionAsync(async () => await _orderService.RemoveAsync(orderId));

        #endregion Act(When)

        #region Assert(Then)

        Assert.Null(exception);

        _orderRepositoryMock
            .Verify(
                repository => repository.Remove(It.Is<Order>(order =>
                    order.OrderProducts.Count() == 1 &&
                    order.UserId == order.UserId)),
                Times.Once());

        #endregion Assert(Then)
    }

    [Fact]
    public async Task RemoveOrder_UsingInvalidOrderId_ThenShouldGiveAnException()
    {
        #region Arrange(Given)

        // Produto
        var productId = Guid.NewGuid();
        var productName = "Test";
        var productDesc = "Test description";
        var orderId = Guid.NewGuid();
        decimal productPrice = 30;
        int quantity = 1;
        var productCategory = ProductCategory.Sobremesa;

        var product = new Product(productName, productDesc, productPrice, productCategory);

        // Order product
        OrderProduct orderProduct = new OrderProduct(orderId, productId, productPrice, quantity, product);

        var userId = Guid.NewGuid();
        var orderStatus = OrderStatus.EmPreparacao;

        //Pedido
        Order order = new(userId);
        order.AddProduct(orderProduct);
        order.SetStatus(orderStatus);

        var orderList = new List<Order>
        {
            order
        };

        _orderRepositoryMock
            .Setup(x => x.GetOrders(null, null))
            .ReturnsAsync(orderList);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        #endregion Arrange(Given)

        #region Act(When)

        var exception = await Record.ExceptionAsync(async () => await _orderService.RemoveAsync(Guid.NewGuid()));

        #endregion Act(When)

        #region Assert(Then)

        Assert.NotNull(exception);
        Assert.Equal("Pedido inválido.", exception.Message);

        _orderRepositoryMock
            .Verify(
                repository => repository.Remove(It.Is<Order>(order =>
                    order.OrderProducts.Count() == 1 &&
                    order.UserId == order.UserId)),
                Times.Never());

        #endregion Assert(Then)
    }

    [Fact]
    public async Task Update_Order_Status()
    {
        #region Arrange(Given)

        // Produto
        var productId = Guid.NewGuid();
        var productName = "Test";
        var productDesc = "Test description";
        var orderId = Guid.NewGuid();
        decimal productPrice = 30;
        int quantity = 1;
        var productCategory = ProductCategory.Sobremesa;

        var product = new Product(productName, productDesc, productPrice, productCategory);

        // Order product
        OrderProduct orderProduct = new OrderProduct(orderId, productId, productPrice, quantity, product);

        var userId = Guid.NewGuid();
        var orderStatus = OrderStatus.EmPreparacao;

        //Pedido
        Order order = new(userId);
        order.AddProduct(orderProduct);
        order.SetStatus(orderStatus);

        var orderList = new List<Order>
        {
            order
        };

        _orderRepositoryMock
            .Setup(x => x.GetOrders(null, null))
            .ReturnsAsync(orderList);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        #endregion Arrange(Given)

        #region Act(When)

        var exception = await Record.ExceptionAsync(async () => await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Pronto));

        #endregion Act(When)

        #region Assert(Then)

        Assert.Null(exception);

        _orderRepositoryMock
            .Verify(
                repository => repository.UpdateAsync(It.Is<Order>(order =>
                    order.OrderProducts.Count() == 1 &&
                    order.UserId == order.UserId)),
                Times.Once());

        #endregion Assert(Then)
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidOrderId_ThenShouldGiveAnException()
    {
        #region Arrange(Given)

        // Produto
        var productId = Guid.NewGuid();
        var productName = "Test";
        var productDesc = "Test description";
        var orderId = Guid.NewGuid();
        decimal productPrice = 30;
        int quantity = 1;
        var productCategory = ProductCategory.Sobremesa;

        var product = new Product(productName, productDesc, productPrice, productCategory);

        // Order product
        OrderProduct orderProduct = new OrderProduct(orderId, productId, productPrice, quantity, product);

        var userId = Guid.NewGuid();
        var orderStatus = OrderStatus.EmPreparacao;

        //Pedido
        Order order = new(userId);
        order.AddProduct(orderProduct);
        order.SetStatus(orderStatus);

        var orderList = new List<Order>
        {
            order
        };

        _orderRepositoryMock
            .Setup(x => x.GetOrders(null, null))
            .ReturnsAsync(orderList);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        #endregion Arrange(Given)

        #region Act(When)

        var exception = await Record.ExceptionAsync(async () => await _orderService.UpdateOrderStatusAsync(Guid.NewGuid(), OrderStatus.Pronto));

        #endregion Act(When)

        #region Assert(Then)

        Assert.NotNull(exception);
        Assert.Equal("Pedido inválido.", exception.Message);

        _orderRepositoryMock
            .Verify(
                repository => repository.UpdateAsync(It.Is<Order>(order =>
                    order.OrderProducts.Count() == 1 &&
                    order.UserId == order.UserId)),
                Times.Never());

        #endregion Assert(Then)
    }

    [Fact]
    public async Task UpdateStatus_WithSameStatus_ThenShouldGiveAnException()
    {
        #region Arrange(Given)

        // Produto
        var productId = Guid.NewGuid();
        var productName = "Test";
        var productDesc = "Test description";
        var orderId = Guid.NewGuid();
        decimal productPrice = 30;
        int quantity = 1;
        var productCategory = ProductCategory.Sobremesa;

        var product = new Product(productName, productDesc, productPrice, productCategory);

        // Order product
        OrderProduct orderProduct = new OrderProduct(orderId, productId, productPrice, quantity, product);

        var userId = Guid.NewGuid();
        var orderStatus = OrderStatus.EmPreparacao;

        //Pedido
        Order order = new(userId);
        order.AddProduct(orderProduct);
        order.SetStatus(orderStatus);

        var orderList = new List<Order>
        {
            order
        };

        _orderRepositoryMock
            .Setup(x => x.GetOrders(null, null))
            .ReturnsAsync(orderList);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        #endregion Arrange(Given)

        #region Act(When)

        var exception = await Record.ExceptionAsync(async () => await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.EmPreparacao));

        #endregion Act(When)

        #region Assert(Then)

        Assert.NotNull(exception);
        Assert.Equal($"Pedido já possui status {orderStatus.GetDescription()}", exception.Message);

        _orderRepositoryMock
            .Verify(
                repository => repository.UpdateAsync(It.Is<Order>(order =>
                    order.OrderProducts.Count() == 1 &&
                    order.UserId == order.UserId)),
                Times.Never());

        #endregion Assert(Then)
    }
}