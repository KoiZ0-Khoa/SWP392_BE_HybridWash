using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    public partial class UpdateTimeSlotCapacity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.TimeSlots', 'BikeCapacity') IS NULL
                BEGIN
                    EXEC(N'ALTER TABLE dbo.TimeSlots
                        ADD BikeCapacity INT NOT NULL
                        CONSTRAINT DF_TimeSlots_BikeCapacity DEFAULT (5);');
                END;

                IF COL_LENGTH('dbo.TimeSlots', 'CarCapacity') IS NULL
                BEGIN
                    EXEC(N'ALTER TABLE dbo.TimeSlots
                        ADD CarCapacity INT NOT NULL
                        CONSTRAINT DF_TimeSlots_CarCapacity DEFAULT (2);');
                END;

                IF COL_LENGTH('dbo.TimeSlots', 'Capacity') IS NOT NULL
                BEGIN
                    DECLARE @CapacityDefaultConstraint SYSNAME;

                    SELECT @CapacityDefaultConstraint = dc.name
                    FROM sys.default_constraints AS dc
                    INNER JOIN sys.columns AS c
                        ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('dbo.TimeSlots')
                      AND c.name = 'Capacity';

                    IF @CapacityDefaultConstraint IS NOT NULL
                    BEGIN
                        DECLARE @DropCapacityDefaultSql NVARCHAR(MAX);
                        SET @DropCapacityDefaultSql = N'ALTER TABLE dbo.TimeSlots DROP CONSTRAINT '
                            + QUOTENAME(@CapacityDefaultConstraint) + N';';
                        EXEC sys.sp_executesql @DropCapacityDefaultSql;
                    END;

                    EXEC(N'ALTER TABLE dbo.TimeSlots DROP COLUMN Capacity;');
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.TimeSlots', 'Capacity') IS NULL
                BEGIN
                    EXEC(N'ALTER TABLE dbo.TimeSlots
                        ADD Capacity INT NOT NULL
                        CONSTRAINT DF_TimeSlots_Capacity DEFAULT (0);');
                END;

                IF COL_LENGTH('dbo.TimeSlots', 'BikeCapacity') IS NOT NULL
                BEGIN
                    DECLARE @BikeDefaultConstraint SYSNAME;

                    SELECT @BikeDefaultConstraint = dc.name
                    FROM sys.default_constraints AS dc
                    INNER JOIN sys.columns AS c
                        ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('dbo.TimeSlots')
                      AND c.name = 'BikeCapacity';

                    IF @BikeDefaultConstraint IS NOT NULL
                    BEGIN
                        DECLARE @DropBikeDefaultSql NVARCHAR(MAX);
                        SET @DropBikeDefaultSql = N'ALTER TABLE dbo.TimeSlots DROP CONSTRAINT '
                            + QUOTENAME(@BikeDefaultConstraint) + N';';
                        EXEC sys.sp_executesql @DropBikeDefaultSql;
                    END;

                    EXEC(N'ALTER TABLE dbo.TimeSlots DROP COLUMN BikeCapacity;');
                END;

                IF COL_LENGTH('dbo.TimeSlots', 'CarCapacity') IS NOT NULL
                BEGIN
                    DECLARE @CarDefaultConstraint SYSNAME;

                    SELECT @CarDefaultConstraint = dc.name
                    FROM sys.default_constraints AS dc
                    INNER JOIN sys.columns AS c
                        ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('dbo.TimeSlots')
                      AND c.name = 'CarCapacity';

                    IF @CarDefaultConstraint IS NOT NULL
                    BEGIN
                        DECLARE @DropCarDefaultSql NVARCHAR(MAX);
                        SET @DropCarDefaultSql = N'ALTER TABLE dbo.TimeSlots DROP CONSTRAINT '
                            + QUOTENAME(@CarDefaultConstraint) + N';';
                        EXEC sys.sp_executesql @DropCarDefaultSql;
                    END;

                    EXEC(N'ALTER TABLE dbo.TimeSlots DROP COLUMN CarCapacity;');
                END;
                """);
        }
    }
}
