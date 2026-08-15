using HybridWash.Repositories.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations;

[DbContext(typeof(AutowashContext))]
[Migration("20260815070500_UpdateBookingStatusConstraint")]
public partial class UpdateBookingStatusConstraint : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += N'ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [' + name + N'];' + CHAR(13)
            FROM sys.check_constraints
            WHERE parent_object_id = OBJECT_ID(N'[dbo].[Bookings]')
              AND parent_column_id = COLUMNPROPERTY(OBJECT_ID(N'[dbo].[Bookings]'), N'Status', 'ColumnId');

            IF @sql <> N''
            BEGIN
                EXEC sp_executesql @sql;
            END
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
