Feature: Daily Consolidated Report

    As a user of the cash flow system
    I want to obtain a daily consolidated report
    So that I can view the total credits, debits, and the overall total for a specific day

    Scenario Outline: Successfully generate daily consolidated report
        Given <date> as a consultation date
        And there are TotalCredits <credits> and TotalDebits <debits> values for this date
        When I request the consolidated report for the consultation date
        Then the response status code of the Consolidated endpoint should be 200
        And the report should contain:
            | date       | totalCredits | totalDebits | isClosed | 
            | <date>     | <credits>    | <debits>    | <closed> |
        And the NetBalance value should be the result of credits-debits
        # And the value of the IsClosed property must be <closed>

        Examples:
            | date         | credits | debits | closed |
            | "2025-05-31" | 200.00  | 100.00 | true  |
            # | "2025-05-30" | 500.00  | 300.00 | true  |

    # Scenario Outline: Generate report for date with no entries
    #     Given there are no entries for the consultation date <date>
    #     When the current date is <currentDate>
    #     And I request the consolidated report for the consultation date
    #     Then the response status code of the Consolidated endpoint should be 200
    #     And the report should contain:
    #         | date       | totalCredits | totalDebits | isClosed |
    #         | <date>     | 0.00         | 0.00        | <closed> |
    #     And the NetBalance value should be the result of credits-debits

    #     Examples:
    #         | date       | currentDate | closed |
    #         | 2025-06-02 | 2025-06-01  | false  |

    # Scenario Outline: Fail to request report with invalid date
    #     Given I provide an invalid consultation date <date>
    #     When the current date is <currentDate>
    #     And I request the consolidated report for the consultation date
    #     Then the response status code of the Consolidated endpoint should be 400
    #     And the response should contain an error message <message>

    #     Examples:
    #         | date        | currentDate | message                                 |
    #         | "invalid"   | 2025-06-01  | "The provided date is invalid."         |
    #         | ""          | 2025-06-01  | "The date must be provided."            |
