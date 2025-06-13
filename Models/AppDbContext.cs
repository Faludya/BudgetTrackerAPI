using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<ImportSession> ImportSessions { get; set; }
        public DbSet<ImportedTransaction> ImportedTransactions { get; set; }
        public DbSet<CategorySuggestion> CategorySuggestions{ get; set; }
        public DbSet<BudgetTemplate> BudgetTemplates { get; set; }
        public DbSet<BudgetTemplateItem> BudgetTemplateItems { get; set; }
        public DbSet<UserBudget> UserBudgets { get; set; }
        public DbSet<UserBudgetItem> UserBudgetItems { get; set; }
        public DbSet<DashboardLayout> DashboardLayouts { get; set; }
        public DbSet<CategoryKeywordMapping> CategoryKeywordMappings { get; set; }
        public DbSet<CurrencyExchangeRate> CurrencyExchangeRates { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Self-referencing relationship for Category
            modelBuilder.Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascading delete

            // Relationship: Category -> Transactions
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CategoryId);

            // Relationship: Currency -> Transactions
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Currency)
                .WithMany()
                .HasForeignKey(t => t.CurrencyId);

            // Relationship: ApplicationUser -> Transactions
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId);

            // Relationship: ApplicationUser -> Categories
            modelBuilder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete categories when user is deleted

            // BudgetTemplate -> BudgetTemplateItem (1-to-many)
            modelBuilder.Entity<BudgetTemplate>()
                .HasMany(bt => bt.Items)
                .WithOne(i => i.BudgetTemplate)
                .HasForeignKey(i => i.BudgetTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserBudget -> UserBudgetItem (1-to-many)
            modelBuilder.Entity<UserBudget>()
                .HasMany(ub => ub.BudgetItems)
                .WithOne(i => i.UserBudget)
                .HasForeignKey(i => i.UserBudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CategorySuggestion>()
                .HasOne(cs => cs.ImportedTransaction)
                .WithMany()
                .HasForeignKey(cs => cs.ImportedTransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DashboardLayout>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            modelBuilder.Entity<CurrencyExchangeRate>()
                .HasOne(c => c.BaseCurrency)
                .WithMany()
                .HasForeignKey(c => c.BaseCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CurrencyExchangeRate>()
                .HasOne(c => c.TargetCurrency)
                .WithMany()
                .HasForeignKey(c => c.TargetCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
