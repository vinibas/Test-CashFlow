Feature: Daily Consolidated Report

    As a user of the cash flow system
    I want to obtain a daily consolidated report
    So that I can view the total credits, debits, and the overall total for a specific day


    Background:
        Given I have the following entries in my database:
            | date         | value | type |
            | "2025-04-01" | 10.50 | "C"  |
            | "2025-04-02" | 5.50  | "D"  |
            | "2025-04-02" | 13.11 | "C"  |
            | "2025-04-02" | 20.08 | "C"  |
            | "2025-04-02" | 2.00  | "D"  |
            | "2025-04-03" | 5.00  | "D"  |


    Scenario Outline: Successfully generate daily consolidated report
        Given <date> as a consultation date
        When I request the consolidated report of the type <type> for the consultation date
        Then the response status code of the Consolidated endpoint should be 200
        And the report should contain:
            | date   | totalCredits | totalDebits | netBalance | isClosed |
            | <date> | <credits>    | <debits>    | <total>    | <closed> |
        And the NetBalance value should be the result of credits-debits

        Examples:
            | type       | date         | credits | debits | total | closed |
            | "resumed"  | "2025-04-02" | 33.19   | 7.50   | 25.69 | true   |
            | "extended" | "2025-04-02" | 33.19   | 7.50   | 25.69 | true   |


    Scenario Outline: Generate report for date with no entries
        Given <date> as a consultation date
        And there are no entries for this date
        When I request the consolidated report of the type <type> for the consultation date
        Then the response status code of the Consolidated endpoint should be 200
        And the report should contain:
            | date   | totalCredits | totalDebits | netBalance | isClosed |
            | <date> | 0.00         | 0.00        | 0.00       | <closed> |
        And the NetBalance value should be the result of credits-debits

        Examples:
            | type       | date         | closed |
            | "resumed"  | "2025-05-01" | true   |
            | "extended" | "2025-05-01" | true   |


    Scenario: Fail to request report with invalid date
        Given "invalid" as a consultation date
        When I request the consolidated report of the type <type> for the consultation date
        Then the response status code of the Consolidated endpoint should be 400
        And the response should contain an error message "The date format is invalid. Use 'yyyy-MM-dd'."

        Examples:
            | type       |
            | "resumed"  |
            | "extended" |