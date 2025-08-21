DROP DATABASE IF EXISTS `GameDb`;
CREATE DATABASE IF NOT EXISTS `GameDb`;
USE `GameDb`;

-- ----------------------------------------------------------
--  유저 인벤토리 캐릭터가 장착한 아이템 정보 테이블
-- ----------------------------------------------------------
DROP TABLE IF EXISTS `character_equipment_item`;
CREATE TABLE IF NOT EXISTS `character_equipment_item` (
    `character_id` BIGINT NOT NULL COMMENT '캐릭터 ID',
    `item_id`      BIGINT NOT NULL COMMENT '아이템 ID',
    PRIMARY KEY (`character_id`, `item_id`)
);

-- ----------------------------------------------------------
--  유저 인벤토리 캐릭터가 장착한 룬 정보 테이블
-- ----------------------------------------------------------
DROP TABLE IF EXISTS `character_equipment_rune`;
CREATE TABLE IF NOT EXISTS `character_equipment_rune` (
    `character_id` BIGINT NOT NULL COMMENT '캐릭터 ID',
    `rune_id`      BIGINT NOT NULL COMMENT '룬 ID',
    PRIMARY KEY (`character_id`, `rune_id`)
);

-- ----------------------------------------------------------
--  유저 월간 출석 정보 테이블
-- ----------------------------------------------------------
DROP TABLE IF EXISTS `user_attendance_month`;
CREATE TABLE IF NOT EXISTS `user_attendance_month` (
    `id`                        BIGINT      AUTO_INCREMENT PRIMARY KEY      COMMENT '고유 ID',
    `user_id`                   BIGINT      NOT NULL                        COMMENT '유저 ID',
    `last_attendance_date`      INT         NOT NULL                        COMMENT '마지막 출석 일자',
    `start_update_date`         DATETIME    NOT NULL                        COMMENT '시작 업데이트 날짜',
    `last_update_date`          DATETIME    NOT NULL                        COMMENT '마지막 업데이트 날짜'
);

-- ----------------------------------------------------------
--  유저 스테이지 클리어 정보 테이블
-- ----------------------------------------------------------
DROP TABLE IF EXISTS `user_clear_stage`;
CREATE TABLE IF NOT EXISTS `user_clear_stage` (
    `id`                BIGINT         AUTO_INCREMENT PRIMARY KEY      COMMENT '고유 ID',
    `user_id`           BIGINT         NOT NULL                        COMMENT '유저 ID',
    `stage_Code`        INT            NOT NULL                        COMMENT '스테이지 코드',
    `clear_count`       INT            NOT NULL                        COMMENT '클리어 횟수',
    `first_clear_date`  DATETIME       NOT NULL                        COMMENT '첫 클리어 날짜',
    `last_clear_date`   DATETIME       NOT NULL                        COMMENT '가장 최근 클리어 날짜'
);

-- ----------------------------------------------------------
--  유저 게임 데이터 테이블
-- ----------------------------------------------------------
DROP TABLE IF EXISTS `user_game_data`;
CREATE TABLE IF NOT EXISTS `user_game_data` (
    `user_id`                       BIGINT      AUTO_INCREMENT PRIMARY KEY  COMMENT '유저 ID',
    `gold`                          INT         NOT NULL                    COMMENT '골드',
    `gem`                           INT         NOT NULL                    COMMENT '유료 재화',
    `exp`                           INT         NOT NULL                    COMMENT '경험치',
    `level`                         INT         NOT NULL                    COMMENT '레벨',
    `total_monster_kill_count`      INT         NOT NULL                    COMMENT '총 몬스터 처치 횟수',
    `total_clear_count`             INT         NOT NULL                    COMMENT '총 스테이지 클리어 횟수'
);

-- ----------------------------------------------------------
--  유저 인벤토리 아이템 데이터 테이블
-- ----------------------------------------------------------
DROP TABLE IF EXISTS `user_inventory_item`;
CREATE TABLE IF NOT EXISTS `user_inventory_item` (
    `item_id`           BIGINT         AUTO_INCREMENT PRIMARY KEY          COMMENT '아이템 ID',
    `user_id`           BIGINT         NOT NULL                            COMMENT '유저 ID',
    `item_code`         BIGINT         NOT NULL                            COMMENT '아이템 코드',
    `level`             INT            NOT NULL                            COMMENT '아이템 레벨'
);

-- ----------------------------------------------------------
--  유저 인벤토리 룬 데이터 테이블
-- ----------------------------------------------------------
DROP TABLE IF EXISTS `user_inventory_rune`;
CREATE TABLE IF NOT EXISTS `user_inventory_rune` (
    `rune_id`           BIGINT         AUTO_INCREMENT PRIMARY KEY          COMMENT '룬 ID',
    `user_id`           BIGINT         NOT NULL                            COMMENT '유저 ID',
    `rune_code`         BIGINT         NOT NULL                            COMMENT '룬 코드',
    `level`             INT            NOT NULL                            COMMENT '룬 레벨'
);

-- ----------------------------------------------------------
--  유저 인벤토리 캐릭터 데이터 테이블
-- ----------------------------------------------------------
DROP TABLE IF EXISTS `user_inventory_character`;
CREATE TABLE IF NOT EXISTS `user_inventory_character` (
    `character_id`              BIGINT         AUTO_INCREMENT  PRIMARY KEY     COMMENT '캐릭터 ID',
    `user_id`                   BIGINT         NOT NULL                        COMMENT '유저 ID',
    `character_code`            BIGINT         NOT NULL                        COMMENT '캐릭터 코드',
    `level`                     INT            NOT NULL                        COMMENT '캐릭터 레벨'
);

-- ----------------------------------------------------------
--  유저 퀘스트 진행 데이터 테이블
-- ----------------------------------------------------------
DROP TABLE IF EXISTS `user_quest_inprogress`;
CREATE TABLE IF NOT EXISTS `user_quest_inprogress` (
    `quest_inprogress_id`           BIGINT         AUTO_INCREMENT      PRIMARY KEY     COMMENT '고유 ID',
    `user_id`                       BIGINT         NOT NULL                            COMMENT '유저 ID',
    `quest_code`                    BIGINT         NOT NULL                            COMMENT '퀘스트 코드',
    `progress`                      INT            NOT NULL                            COMMENT '퀘스트 진행 사항',
    `expire_date`                   DATETIME       NOT NULL                            COMMENT '퀘스트 만료 시간'
);

-- ----------------------------------------------------------
--  유저 퀘스트 완료 데이터 테이블
-- ----------------------------------------------------------
DROP TABLE IF EXISTS `user_quest_complete`;
CREATE TABLE IF NOT EXISTS `user_quest_complete` (
    `quest_complete_ id`            BIGINT      AUTO_INCREMENT      PRIMARY KEY     COMMENT '고유 ID',
    `user_id`                       BIGINT      NOT NULL                            COMMENT '유저 ID',
    `quest_code`                    BIGINT      NOT NULL                            COMMENT '퀘스트 코드',
    `complete_date`                 DATETIME    NOT NULL                            COMMENT '퀘스트 완료 시간',
    `earn_reward`                   BOOLEAN     NOT NULL            DEFAULT FALSE   COMMENT '보상 획득 여부'
);
