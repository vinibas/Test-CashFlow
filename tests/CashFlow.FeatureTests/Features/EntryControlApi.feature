Feature: Entry Control API

    As a user of the cash flow system
    I want to create a new entry (credit or debit) via the API
    So that I can control my cash flow movements

    Scenario Outline: Successfully create a new entry
        Given I have a entry with value <value> and type <type>
        When I send a POST request to Entry Control endpoint with this entry
        Then the response status code should be 201
        And the entry should be created successfully

        Examples:
            | value  | type |
            | 123.45 | 'C'  |
            | 678.90 | 'D'  |

    Scenario Outline: Fail to create entry with invalid type
        Given I have a entry with value <value> and type <type>
        When I send a POST request to Entry Control endpoint with this entry
        Then the response status code should be 400
        And the response should be an ErrorDetails with the messages <messages>
        And the entry should not be created
        
        Examples:
            | value  | type | messages                                                       |
            | 0      | 'C'  | "The entry value must be greater than zero."                   |
            | 123.45 | 'A'  | "The entry type must be only 'C' for credit or 'D' for debit." |


    # Scenario: Successfully create a new entry (credit)
    #     Given I have a valid entry with value 123.45 and type 'C'
    #     When I send a POST request to Entry Control endpoint with this entry
    #     Then the response status code should be 201
    #     And the entry should be created successfully

    # Scenario: Successfully create a new entry (debit)
    #     Given I have a valid entry with value 50.00 and type "D"
    #     When I send a POST request to "/api/v1/EntryControl" with this entry
    #     Then the response status code should be 201
    #     And the entry should be created successfully

    # Scenario: Fail to create entry with invalid type
    #     Given I have an entry with value 100.00 and type "X"
    #     When I send a POST request to "/api/v1/EntryControl" with this entry
    #     Then the response status code should be 400
    #     And the entry should not be created

    # Scenario: Fail to create entry with missing value
    #     Given I have an entry with no value and type "C"
    #     When I send a POST request to "/api/v1/EntryControl" with this entry
    #     Then the response status code should be 400
    #     And the entry should not be created

    # Scenario: Fail to create entry with missing type
    #     Given I have an entry with value 100.00 and no type
    #     When I send a POST request to "/api/v1/EntryControl" with this entry
    #     Then the response status code should be 400
    #     And the entry should not be created