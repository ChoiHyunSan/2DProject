DROP DATABASE IF EXISTS `AccountDB`;
CREATE DATABASE IF NOT EXISTS `AccountDB`;
USE `AccountDB`;

-----------------------------------------------------------
-- 계정 정보 테이블
-----------------------------------------------------------
DROP TABLE IF EXISTS `user_account`;
CREATE TABLE IF NOT EXISTS `user_account` (
    `account_id`      BIGINT          PRIMARY KEY     AUTO_INCREMENT      COMMENT `계정 UID`,
    `user_id`         BIGINT          NOT NULL,       UNIQUE              COMMENT `유저 UID`,  
    `email`           VARCHAR(50)     NOT NULL        UNIQUE              COMMENT `유저 이메일`,
    `password`        VARCHAR(100)    NOT NULL                            COMMENT `유저 비밀번호`,
    `salt_value`      VARCHAR(100)    NOT NULL                            COMMENT `비밀번호 암호화 값`,
    `create_date`     DATETIME        DEFAULT CURRENT_TIMESTAMP()         COMMENT `가입 일시`
);