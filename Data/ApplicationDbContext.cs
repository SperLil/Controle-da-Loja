using LojaApp.Models; //Para encontrar a classe 'Produto'
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace LojaApp.Data
{
    //ApplicationDbContext henda de DbContext
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        //Informa que tem uma tabela chamada Produtos que é representada pela classe Produto
        public DbSet<Produto> Produtos { get; set; }

         // Nova lista
            public DbSet<Venda> Vendas { get; set; }
            public DbSet<Cliente> Clientes { get; set; }
            public DbSet<ItemVenda> ItensVenda { get; set; }

    }
}
