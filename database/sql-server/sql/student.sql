CREATE TABLE STUDENT
(
    id         BIGINT PRIMARY KEY IDENTITY (1, 1),
    first_name VARCHAR(50) NOT NULL,
    last_name  VARCHAR(50) NOT NULL,
    updated    DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
);

INSERT INTO STUDENT(first_name, last_name)
values ('hello', 'world');