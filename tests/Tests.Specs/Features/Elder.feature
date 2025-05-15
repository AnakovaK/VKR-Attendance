Feature: Elder

Background:
    Given a global semester named Semester


Scenario: Get available visiting logs
    Given exists visiting log with students
    | visiting log title | student id | status |
    | vl1                | usr1       | elder  |
    Given the student usr1
    When receive available visiting logs
    Then received visiting log vl1

Scenario: Get visiting log composition
    Given exists visiting log with students
    | visiting log title | student id | status  |
    | vl2                | st2        | elder   |
    | vl2                | st3        | student |
    | vl2                | st4        | student |
    | vl2                | st5        | student |
    | vl3                | st6        | student |
    | vl3                | st7        | elder   |
    Given the student st6
    When receive composition from vl3
    Then received composition contains
    | student id |
    | st7        |
    | st6        |
    Given the student st2
    When receive composition from vl2
    Then received composition contains
    | student id |
    | st3        |
    | st2        |
    | st4        |
    | st5        |

Scenario: Create lesson from elder
    Given exists visiting log with students
    | visiting log title | student id | status  |
    | vl4                | st8        | elder   |
    Given exists visiting log with disciplines
    | visiting log title | discipline |
    | vl4                | dis1       |
    Given timeslot n1 from 120 to 150
    Given exists lesson types with statuses
    | lesson type id | lesson type name |
    | lest1          | pr               |
    Given the student st8
    When creating lesson in vl4
    Then lesson created successfully

Scenario: Get lessons for day is correct
    Given exists visiting log with students
    | visiting log title | student id | status  |
    | vl5                | st9        | student |
    Given exists lesson ls1 about math at 2023-08-26 from 18:00 to 19:30 of type practic
    Given exists lesson ls2 about math at 2023-08-26 from 10:40 to 12:10 of type practic
    Given exists lesson ls3 about phys at 2023-08-25 from 09:00 to 10:40 of type practic
    Given exists lesson ls4 about math at 2023-08-25 from 12:40 to 14:10 of type practic
    Given exists lesson ls5 about math at 2023-08-24 from 14:20 to 15:50 of type practic
    And log vl5 contains lessons
    | lesson |
    | ls1    |
    | ls2    |
    | ls3    |
    | ls4    |
    | ls5    |
    Given the student st9
    When receive lessons in vl5 for 2023-08-26
    Then lessons received is
    | lesson |
    | ls2    |
    | ls1    |
    When receive lessons in vl5 for 2023-08-25
    Then lessons received is
    | lesson |
    | ls3    |
    | ls4    |
    When receive lessons in vl5 for 2023-08-24
    Then lessons received is
    | lesson |
    | ls5    |
    When receive lessons in vl5 for 2023-08-27
    Then lessons received is
    | lesson |

# СЦЕНАРИИ ПРЕПОДАВАТЕЛЕЙ
# Пока в этом же файле, так как при попытке вынести в отдельный feature - летят все тесты
Scenario: Get lesson from several logs for teacher
    Given exists lesson teacher_ls1 about teacher_math at 2023-08-28 from 18:00 to 19:30 of type teacher_practic
    Given exists lesson teacher_ls2 about teacher_math at 2023-08-28 from 10:40 to 12:10 of type teacher_practic
    And teacher 123 leading to
        | lesson |
        | teacher_ls1 |
        | teacher_ls2 |
    And log teacher_vl1 contains lessons
        | lesson      |
        | teacher_ls1 |
    And log teacher_vl2 contains lessons
        | lesson      |
        | teacher_ls2 |
    Given the teacher 123
    When receive teacher lessons for 2023-08-28
    Then teacher lessons received is
        | lesson      |
        | teacher_ls2 |
        | teacher_ls1 |
