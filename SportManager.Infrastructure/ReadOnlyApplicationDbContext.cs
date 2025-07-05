using SportManager.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Infrastructure.Persistence;

public class ReadOnlyApplicationDbContext : DbContext, IReadOnlyApplicationDbContext
{
    public ReadOnlyApplicationDbContext(DbContextOptions<ReadOnlyApplicationDbContext> options)
        : base(options)
    {
        // Disable change tracking for better read performance
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    // DbSet properties
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<CustomerSuportTicket> CustomerSuportTickets => Set<CustomerSuportTicket>();
    public DbSet<TicketReply> TicketReplies => Set<TicketReply>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ShippingAddress> ShippingAddresses => Set<ShippingAddress>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ProductReviewComment> ProductReviewComments => Set<ProductReviewComment>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<UserVoucher> UserVouchers => Set<UserVoucher>();
    public DbSet<VoucherUsage> VoucherUsages => Set<VoucherUsage>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<MessageStatus> MessageStatus => Set<MessageStatus>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<CustomerStatisfactionRating> CustomerStatisfactionRatings => Set<CustomerStatisfactionRating>();

    public async Task<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
    {
        return await Set<TEntity>().FindAsync(keyValues);
    }

    public async Task<TEntity> FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) where TEntity : class
    {
        return await Set<TEntity>().FindAsync(keyValues, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply same configurations as in main DbContext
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}