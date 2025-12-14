using LinqAdvancedLab.Data.Entities;
using LinqAdvancedLab.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using LinqAdvancedLab.Data.Repositories;

var options = new DbContextOptionsBuilder<NorthwindContext>()
    .UseSqlServer(
        "Server=(localdb)\\MSSQLLocalDB;Database=Northwind;Trusted_Connection=True;")
    .Options;

using var context = new NorthwindContext(options);

// 🔹 HU1 – Inventario crítico
var query = context.Products
    .Where(p => p.UnitsInStock < 20 &&
                (p.UnitPrice ?? 0) * (p.UnitsInStock ?? 0) > 500)
    .OrderBy(p => p.ProductName)
    .Select(p => new ProductDto(
        p.ProductId,
        p.ProductName,
        p.UnitsInStock ?? 0,
        p.UnitPrice ?? 0
    ))
    .Take(5);

// 🔹 Mostrar resultados
foreach (var p in query)
{
    Console.WriteLine(
        $"{p.ProductName} | Stock: {p.UnitsInStock} | Precio: {p.UnitPrice}");
}

// 🔹 SQL generado
Console.WriteLine("\n--- SQL GENERADO ---");
Console.WriteLine(query.ToQueryString());

// =====================================================
// 🔹 QUERY 2 – JOIN ENTRE TABLAS
// =====================================================

var query2 = context.Products
    .Join(
        context.Categories,
        p => p.CategoryId,
        c => c.CategoryId,
        (p, c) => new ProductCategoryDto(
            p.ProductName,
            c.CategoryName,
            p.UnitsInStock ?? 0
        )
    )
    .OrderBy(p => p.CategoryName)
    .Take(10);

Console.WriteLine("\n--- PRODUCTOS CON CATEGORÍA ---");
foreach (var item in query2)
{
    Console.WriteLine($"{item.ProductName} | {item.CategoryName}");
}

Console.WriteLine("\n--- SQL QUERY 2 ---");
Console.WriteLine(query2.ToQueryString());

// =====================================================
// 🔹 QUERY 3 – GROUP BY + AGREGADOS
// =====================================================
var query3 = context.Products
    .GroupBy(p => p.CategoryId)
    .Join(
        context.Categories,
        g => g.Key,
        c => c.CategoryId,
        (g, c) => new CategoryStockDto(
            c.CategoryName,
            g.Count(),
            g.Sum(p => p.UnitsInStock ?? 0)
        )
    )
    .OrderByDescending(x => x.TotalUnitsInStock);

Console.WriteLine("\n=== QUERY 3: STOCK POR CATEGORÍA ===");
foreach (var item in query3)
{
    Console.WriteLine(
        $"{item.CategoryName} | Productos: {item.TotalProducts} | Stock Total: {item.TotalUnitsInStock}");
}

Console.WriteLine("\n--- SQL QUERY 3 ---");
Console.WriteLine(query3.ToQueryString());

// =====================================================
// 🔹 QUERY 4 – SUBCONSULTA CON Any
// =====================================================

var query4 = context.Categories
    .Where(c =>
        context.Products.Any(p =>
            p.CategoryId == c.CategoryId &&
            p.UnitsInStock < 10))
    .Select(c => c.CategoryName);

Console.WriteLine("\n=== QUERY 4: CATEGORÍAS CON STOCK BAJO ===");
foreach (var name in query4)
{
    Console.WriteLine(name);
}

Console.WriteLine("\n--- SQL QUERY 4 ---");
Console.WriteLine(query4.ToQueryString());

// =====================================================
// 🔹 QUERY 5 – PAGINACIÓN
// =====================================================
int pageNumber = 2; // página que quieres
int pageSize = 5;

var query5 = context.Products
    .OrderBy(p => p.ProductId)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .Select(p => new ProductDto(
        p.ProductId,
        p.ProductName,
        p.UnitsInStock ?? 0,
        p.UnitPrice ?? 0
    ));

Console.WriteLine("\n=== QUERY 5: PAGINACIÓN DE PRODUCTOS ===");
foreach (var p in query5)
{
    Console.WriteLine($"{p.ProductId} - {p.ProductName}");
}

Console.WriteLine("\n--- SQL QUERY 5 ---");
Console.WriteLine(query5.ToQueryString());

// =====================================================
var productRepo = new Repository<Product>(context);

var criticalProducts = productRepo
    .Find(p => p.UnitsInStock < 10)
    .Take(5);

Console.WriteLine("\n=== PRODUCTOS CRÍTICOS (REPO) ===");
foreach (var p in criticalProducts)
{
    Console.WriteLine(p.ProductName);
}