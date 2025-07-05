using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SportManager.Application.Common.Interfaces;
using SportManager.Domain;
using SportManager.Domain.Entity;

namespace SportManager.Infrastructure;
public class AppDbContext : DbContext, IApplicationDbContext
{
    private IDbContextTransaction _currentTransaction;
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductReviewComment> ProductReviewComments => Set<ProductReviewComment>();
    public DbSet<CustomerSuportTicket> CustomerSuportTickets => Set<CustomerSuportTicket>();
    public DbSet<TicketReply> TicketReplies => Set<TicketReply>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ShippingAddress> ShippingAddresses => Set<ShippingAddress>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<MessageStatus> MessageStatus => Set<MessageStatus>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<CustomerStatisfactionRating> CustomerStatisfactionRatings => Set<CustomerStatisfactionRating>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<UserVoucher> UserVouchers => Set<UserVoucher>();
    public DbSet<VoucherUsage> VoucherUsages => Set<VoucherUsage>();

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is EntityBase<Guid>);

        foreach (var entry in entries)
        {
            var entity = (EntityBase<Guid>)entry.Entity;
            entity.CreatedAt = DateTime.UtcNow;
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }


    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _currentTransaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Brand
        modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
                entity.Property(b => b.Slug).HasMaxLength(100);
                entity.Property(b => b.LogoUrl);
                entity.Property(b => b.Website);
                entity.Property(b => b.Country);
                entity.Property(b => b.CountryId);
                entity.Property(b => b.City);
                entity.Property(b => b.Descriptions).HasMaxLength(500);
                entity.Property(b => b.IsActive).HasDefaultValue(true);
                entity.Property(b => b.FoundedYear);

                entity.HasMany(b => b.Products)
                    .WithOne(p => p.Brand)
                    .HasForeignKey(p => p.BrandId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

        // Cart
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CartItem
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.Quantity).IsRequired();
            entity.Property(ci => ci.UnitPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId);

            entity.HasOne(ci => ci.ProductVariant)
                .WithMany()
                .HasForeignKey(ci => ci.ProductVariantId);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description);
            entity.Property(c => c.Logo);
        });

        // ProductCategory
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(pc => pc.Id);
            entity.HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.ProductId);

            entity.HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId);
        });

        // CustomerSupportTicket
        modelBuilder.Entity<CustomerSuportTicket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Status);
            entity.Property(e => e.Priority);
            entity.Property(e => e.ClosedAt);

            entity.HasOne(e => e.Customer)
                .WithMany(u => u.SupportTickets)
                .HasForeignKey(e => e.CustomerId);

            entity.HasMany(e => e.Replies)
                .WithOne(r => r.Ticket)
                .HasForeignKey(r => r.TicketId);
        });

        // TicketReply
        modelBuilder.Entity<TicketReply>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.RepliedAt);

            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.Replies)
                .HasForeignKey(e => e.TicketId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Notes);
            entity.Property(o => o.State);
            entity.Property(o => o.OrderDate);

            entity.HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId);

            entity.HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId);

            entity.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.Quantity);
            entity.Property(oi => oi.UnitPrice);
            entity.Property(oi => oi.State);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Method);
            entity.Property(p => p.Status);
            entity.Property(p => p.PaidAt);
        });

        // ShippingAddress
        modelBuilder.Entity<ShippingAddress>(entity =>
        {
            entity.HasKey(sa => sa.Id);
            entity.Property(sa => sa.RecipientName);
            entity.Property(sa => sa.Phone);
            entity.Property(sa => sa.AddressLine);
            entity.Property(sa => sa.Country);
            entity.Property(sa => sa.CountryId);
            entity.Property(sa => sa.City);
            entity.Property(sa => sa.CityId);
            entity.Property(sa => sa.District);
            entity.Property(sa => sa.Ward);
            entity.Property(sa => sa.PostalCode);
            entity.Property(sa => sa.IsDefault);

            entity.HasOne(sa => sa.Customer)
                .WithMany(u => u.ShippingAddresses)
                .HasForeignKey(sa => sa.CustomerId);
        });

        // Supplier
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired();
            entity.Property(s => s.Description);
            entity.Property(s => s.ContactPhone);
            entity.Property(s => s.Address);
            entity.Property(s => s.City);
            entity.Property(s => s.Region);
            entity.Property(s => s.PostalCode);
            entity.Property(s => s.Country);
            entity.Property(s => s.Phone);
            entity.Property(s => s.IsActive);
            entity.Property(s => s.Fax);
            entity.Property(s => s.ContactEmail);

            entity.HasMany(s => s.Products)
                .WithOne(p => p.Supplier)
                .HasForeignKey(p => p.SupplierId);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Description);
            entity.Property(p => p.Images);

            entity.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId);

            entity.HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId);
        });

        // ProductReview
        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(pr => pr.Id);
            entity.Property(pr => pr.Rating);
            entity.Property(pr => pr.Comment);
            entity.Property(pr => pr.IsVerifiedPurchase);

            entity.HasOne(pr => pr.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(pr => pr.ProductId);

            entity.HasOne(pr => pr.Customer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(pr => pr.CustomerId);
        });

        // ProductVariant
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(pv => pv.Id);
            entity.Property(pv => pv.Color);
            entity.Property(pv => pv.Name);
            entity.Property(pv => pv.SKU);
            entity.Property(pv => pv.Unit);
            entity.Property(pv => pv.Description);
            entity.Property(pv => pv.Images);
            entity.Property(pv => pv.Size);
            entity.Property(pv => pv.Attribute);
            entity.Property(pv => pv.Price).HasColumnType("decimal(18,2)");
            entity.Property(pv => pv.StockQuantity);

            entity.HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId);
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired();
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.HasMany(u => u.RefreshTokens);
        });

        // UserRole
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => ur.Id);

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            // Primary key
            entity.HasKey(c => c.Id);

            // One-to-one với User
            entity.HasOne(c => c.User)
              .WithOne(u => u.CustomerProfile)
              .HasForeignKey<Customer>(c => c.UserId)
              .OnDelete(DeleteBehavior.Cascade);

            // Các collection navigation properties (Orders, Reviews...) sẽ được EF cấu hình tự động
        });

        modelBuilder.Entity<Voucher>()
                    .HasIndex(v => v.Code)
                    .IsUnique();

        modelBuilder.Entity<VoucherUsage>()
            .HasIndex(vu => new { vu.VoucherId, vu.UserId });

        modelBuilder.Entity<UserVoucher>()
            .HasIndex(uv => new { uv.VoucherId, uv.UserId })
            .IsUnique();
    }
    // seeeding
    //SeedingData.Seed(modelBuilder);
}

