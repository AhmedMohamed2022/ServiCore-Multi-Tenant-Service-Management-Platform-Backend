using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerUserIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Customer.UserId has been part of the EF model (and the
            // checked-in model snapshot) since AddNotifications, but no
            // migration file in this project ever added the column to the
            // database - on a fresh database it genuinely doesn't exist.
            //
            // On some existing databases, though, the column was already
            // added out-of-band (e.g. a migration that added it was
            // later deleted from source control without running
            // `migrations remove`, leaving the physical schema ahead of
            // what's on disk here). A plain AddColumn fails there with
            // "column already exists". Guard with IF NOT EXISTS so this
            // migration is safe to apply regardless of which state a
            // given database is in.
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'UserId' AND Object_ID = OBJECT_ID(N'[Customers]')
)
BEGIN
    ALTER TABLE [Customers] ADD [UserId] uniqueidentifier NULL;
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Customers_UserId' AND object_id = OBJECT_ID(N'[Customers]')
)
BEGIN
    CREATE INDEX [IX_Customers_UserId] ON [Customers] ([UserId]);
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Customers_UserId' AND object_id = OBJECT_ID(N'[Customers]')
)
BEGIN
    DROP INDEX [IX_Customers_UserId] ON [Customers];
END");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'UserId' AND Object_ID = OBJECT_ID(N'[Customers]')
)
BEGIN
    ALTER TABLE [Customers] DROP COLUMN [UserId];
END");
        }
    }
}
