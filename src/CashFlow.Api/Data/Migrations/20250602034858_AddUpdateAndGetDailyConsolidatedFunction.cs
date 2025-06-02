using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashFlow.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateAndGetDailyConsolidatedFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION update_and_get_daily_consolidated(p_date date)
                RETURNS TABLE (
                    "Id" uuid,
                    "Date" date,
                    "TotalCredits" numeric,
                    "TotalDebits" numeric,
                    "LastLineNumberCalculated" bigint
                ) AS $$
                DECLARE
                    v_last_line_number bigint;
                    v_total_credits numeric := 0;
                    v_total_debits numeric := 0;
                    v_max_line_number bigint := 0;
                    v_id uuid;
                BEGIN
                    -- 1. Query DailyConsolidated for the given date
                    SELECT dc."Id", dc."LastLineNumberCalculated"
                    INTO v_id, v_last_line_number
                    FROM "DailyConsolidated" dc
                    WHERE dc."Date" = p_date;

                    -- 2. Query Entries for the given date
                    IF v_last_line_number IS NULL THEN
                        -- No consolidated record for the date, get all entries for the date
                        SELECT
                            COALESCE(SUM(CASE WHEN e."Type" = 'C' THEN e."Value" ELSE 0 END), 0),
                            COALESCE(SUM(CASE WHEN e."Type" = 'D' THEN e."Value" ELSE 0 END), 0),
                            COALESCE(MAX(e."LineNumber"), 0)
                        INTO v_total_credits, v_total_debits, v_max_line_number
                        FROM "Entries" e
                        WHERE e."CreatedAtUtc"::date = p_date;
                    ELSE
                        -- Consolidated exists, get only entries after the last calculated line number
                        SELECT
                            COALESCE(SUM(CASE WHEN e."Type" = 'C' THEN e."Value" ELSE 0 END), 0),
                            COALESCE(SUM(CASE WHEN e."Type" = 'D' THEN e."Value" ELSE 0 END), 0),
                            COALESCE(MAX(e."LineNumber"), v_last_line_number)
                        INTO v_total_credits, v_total_debits, v_max_line_number
                        FROM "Entries" e
                        WHERE e."CreatedAtUtc"::date = p_date
                        AND e."LineNumber" > v_last_line_number;
                    END IF;

                    -- If there are no entries for the date, do not insert or update, just return nothing
                    IF v_max_line_number = 0 THEN
                        RETURN;
                    END IF;

                    -- 3. Update or insert into DailyConsolidated
                    IF v_id IS NULL THEN
                        -- Insert new record
                        INSERT INTO "DailyConsolidated" AS dc ("Id", "Date", "TotalCredits", "TotalDebits", "LastLineNumberCalculated")
                        VALUES (gen_random_uuid(), p_date, v_total_credits, v_total_debits, v_max_line_number)
                        RETURNING 
                            dc."Id", 
                            dc."Date", 
                            dc."TotalCredits", 
                            dc."TotalDebits", 
                            dc."LastLineNumberCalculated"
                        INTO v_id, p_date, v_total_credits, v_total_debits, v_max_line_number;
                    ELSE
                        -- Update existing record
                        UPDATE "DailyConsolidated" dc
                        SET "TotalCredits" = dc."TotalCredits" + v_total_credits,
                            "TotalDebits" = dc."TotalDebits" + v_total_debits,
                            "LastLineNumberCalculated" = v_max_line_number
                        WHERE dc."Id" = v_id
                        RETURNING 
                            dc."Id", 
                            dc."Date", 
                            dc."TotalCredits", 
                            dc."TotalDebits", 
                            dc."LastLineNumberCalculated"
                        INTO v_id, p_date, v_total_credits, v_total_debits, v_max_line_number;
                    END IF;

                    -- Return the updated or inserted record
                    RETURN QUERY
                    SELECT v_id, p_date, v_total_credits, v_total_debits, v_max_line_number;
                END;
                $$ LANGUAGE plpgsql;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS update_and_get_daily_consolidated(date);");
        }
    }
}
