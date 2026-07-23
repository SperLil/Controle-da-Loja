using LojaApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LojaApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ItemVenda> ItensVenda { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pagamento>().ToTable("Pagamentos");

            modelBuilder.Entity<Venda>()
                .Ignore(v => v.TotalPago)
                .Ignore(v => v.ValorRestante)
                .Ignore(v => v.EmAberto);
        }
    }
}