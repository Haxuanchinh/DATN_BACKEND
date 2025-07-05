using Microsoft.EntityFrameworkCore.ChangeTracking;
using SportManager.Domain.Entity;

namespace SportManager.Application.Common.Interfaces;

public interface IReadOnlyApplicationDbContext
{
    // DbSet properties
    DbSet<Domain.Entity.Brand> Brands { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Domain.Entity.Category> Categories { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    public DbSet<ProductReviewComment> ProductReviewComments { get; }

    DbSet<CustomerSuportTicket> CustomerSuportTickets { get; }
    DbSet<TicketReply> TicketReplies { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<ShippingAddress> ShippingAddresses { get; }
    DbSet<Domain.Entity.Supplier> Suppliers { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductReview> ProductReviews { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<SportManager.Domain.Entity.Customer> Customers { get; }
    public DbSet<Message> Messages { get; }
    public DbSet<Conversation> Conversations { get; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; }
    public DbSet<MessageStatus> MessageStatus { get; }
    public DbSet<TicketCategory> TicketCategories { get; }
    public DbSet<CustomerStatisfactionRating> CustomerStatisfactionRatings { get; }
    public DbSet<Voucher> Vouchers { get; }
    public DbSet<UserVoucher> UserVouchers { get; }
    public DbSet<VoucherUsage> VoucherUsages { get; }

    // Read-only operations
    //Task<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class;
    //Task<TEntity> FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) where TEntity : class;

    // Asynchronous query execution
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
public interface IApplicationDbContext : IReadOnlyApplicationDbContext
{
    // Write operations
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;

    // For bulk operations
    void AddRange(params object[] entities);
    void UpdateRange(params object[] entities);
    void RemoveRange(params object[] entities);

    // Asynchronous operations
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

    // Transaction support
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}