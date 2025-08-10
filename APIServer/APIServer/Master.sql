DROP DATABASE IF EXISTS `MasterDB`;
CREATE DATABASE IF NOT EXISTS `MasterDB`;
USE `MasterDB`;

------------------------------------------------------------
-- 월간 출석 보상 데이터
------------------------------------------------------------
DROP TABLE IF EXISTS `attendance_reward_month`;
CREATE TABLE IF NOT EXISTS `attendance_reward_month` (
    `day`        INT         NOT NULL        PRIMARY KEY        COMMENT '출석 일자',
    `item_code`  BIGINT      NOT NULL                           COMMENT '아이템 식별 코드',
    `count`      INT         NOT NULL                           COMMENT '아이템 보상 개수'
);

------------------------------------------------------------
-- 주간 출석 보상 데이터
------------------------------------------------------------
DROP TABLE IF EXISTS `attendance_reward_week`;
CREATE TABLE IF NOT EXISTS `attendance_reward_week` (
    `day`        INT         NOT NULL        PRIMARY KEY        COMMENT '출석 일자',
    `item_code`  BIGINT      NOT NULL                           COMMENT '아이템 식별 코드',
    `count`      INT         NOT NULL                           COMMENT '아이템 보상 개수'
);

------------------------------------------------------------
-- 캐릭터 원본 데이터
------------------------------------------------------------
DROP TABLE IF EXISTS `character_origin_data`;
CREATE TABLE IF NOT EXISTS `character_origin_data` (
    `character_code`   BIGINT        NOT NULL       PRIMARY KEY     COMMENT '캐릭터 식별 코드',
    `name`             VARCHAR(100)  NOT NULL       UNIQUE          COMMENT '캐릭터 이름',
    `description`      VARCHAR(200)  NOT NULL                       COMMENT '캐릭터 설명'
);

------------------------------------------------------------
-- 캐릭터 레벨별 강화 가격
------------------------------------------------------------
DROP TABLE IF EXISTS `character_enhance_data`;
CREATE TABLE IF NOT EXISTS `character_enhance_data` (
    `character_code`   BIGINT       NOT NULL                   COMMENT '캐릭터 식별 코드',
    `level`            INT          NOT NULL                   COMMENT '캐릭터 레벨',
    `attack_damage`    INT          NOT NULL                   COMMENT '추가 공격력',
    `defense`          INT          NOT NULL                   COMMENT '추가 방어력',
    `max_hp`           INT          NOT NULL                   COMMENT '추가 최대 체력',
    `critical_chance`  INT          NOT NULL                   COMMENT '추가 치명타 확률',
    `enhance_price`    INT          NOT NULL                   COMMENT '강화 가격',
    PRIMARY KEY (`character_code`, `level`)
);

------------------------------------------------------------
-- 아이템 원본 데이터
------------------------------------------------------------
DROP TABLE IF EXISTS `item_origin_data`;
CREATE TABLE IF NOT EXISTS `item_origin_data` (
    `item_code`   BIGINT       NOT NULL      PRIMARY KEY      COMMENT '아이템 식별 코드',
    `name`        VARCHAR(100) NOT NULL                       COMMENT '아이템 이름',
    `description` TEXT         NOT NULL                       COMMENT '아이템 설명'
);

------------------------------------------------------------
-- 아이템 레벨별 강화 정보
------------------------------------------------------------
DROP TABLE IF EXISTS `item_enhance_data`;
CREATE TABLE IF NOT EXISTS `item_enhance_data` (
    `item_code`        BIGINT       NOT NULL                   COMMENT '아이템 식별 코드',
    `level`            INT          NOT NULL                   COMMENT '아이템 레벨',
    `attack_damage`    INT          NOT NULL                   COMMENT '추가 공격력',
    `defense`          INT          NOT NULL                   COMMENT '추가 방어력',
    `max_hp`           INT          NOT NULL                   COMMENT '추가 체력',
    `critical_chance`  INT          NOT NULL                   COMMENT '추가 치명타 확률',
    `enhance_price`    INT          NOT NULL                   COMMENT '강화 가격',
    `sell_price`       INT          NOT NULL                   COMMENT '판매 가격',
    PRIMARY KEY (`item_code`, `level`)
);

------------------------------------------------------------
-- 퀘스트 정보 데이터
------------------------------------------------------------
DROP TABLE IF EXISTS `quest_info_data`;
CREATE TABLE IF NOT EXISTS `quest_info_data` (
    `quest_code`  BIGINT       PRIMARY KEY                  COMMENT '퀘스트 식별 코드',
    `name`        VARCHAR(100) NOT NULL                     COMMENT '퀘스트 이름',
    `description` VARCHAR(200) NOT NULL                     COMMENT '퀘스트 설명',
    `reward_gold` INT          NOT NULL                     COMMENT '보상 골드 획득량',
    `reward_gem`  INT          NOT NULL                     COMMENT '보상 잼 획득량',
    `reward_exp`  INT          NOT NULL                     COMMENT '보상 경험치 획득량'
);

------------------------------------------------------------
-- 룬 원본 데이터
------------------------------------------------------------
DROP TABLE IF EXISTS `rune_origin_data`;
CREATE TABLE IF NOT EXISTS `rune_origin_data` (
    `rune_code`   BIGINT       NOT NULL   PRIMARY KEY       COMMENT '룬 식별 코드',
    `name`        VARCHAR(100) NOT NULL   UNIQUE            COMMENT '룬 이름',
    `description` VARCHAR(200) NOT NULL                     COMMENT '룬 설명'
);

------------------------------------------------------------
-- 룬 레벨별 강화 정보
------------------------------------------------------------
DROP TABLE IF EXISTS `rune_enhance_data`;
CREATE TABLE IF NOT EXISTS `rune_enhance_data` (
    `rune_code`        BIGINT       NOT NULL                   COMMENT '룬 식별 코드',
    `level`            BIGINT       NOT NULL                   COMMENT '룬 레벨',
    `attack_damage`    INT          NOT NULL                   COMMENT '추가 공격력',
    `defense`          INT          NOT NULL                   COMMENT '추가 방어력',
    `max_hp`           INT          NOT NULL                   COMMENT '추가 체력',
    `critical_chance`  INT          NOT NULL                   COMMENT '추가 치명타 확률',
    `enhance_count`    INT          NOT NULL                   COMMENT '강화 필요 개수',
    PRIMARY KEY (`rune_code`, `level`)
);

------------------------------------------------------------
-- 스테이지 별 보상 아이템 데이터
------------------------------------------------------------
DROP TABLE IF EXISTS `stage_reward_item`;
CREATE TABLE IF NOT EXISTS `stage_reward_item` (
    `stage_code` BIGINT NOT NULL COMMENT '스테이지 식별 코드',
    `item_code`  BIGINT NOT NULL COMMENT '아이템 식별 코드',
    `level`      INT    NOT NULL COMMENT '아이템 레벨',
    `drop_rate`  INT    NOT NULL COMMENT '드랍 확률',
    PRIMARY KEY (`stage_code`, `item_code`, `level`)
);

------------------------------------------------------------
-- 스테이지 별 보상 룬 데이터
------------------------------------------------------------
DROP TABLE IF EXISTS `stage_reward_rune`;
CREATE TABLE IF NOT EXISTS `stage_reward_rune` (
    `stage_code` BIGINT NOT NULL COMMENT '스테이지 식별 코드',
    `rune_code`  BIGINT NOT NULL COMMENT '룬 식별 코드',
    `drop_rate`  INT    NOT NULL COMMENT '드랍 확률',
    PRIMARY KEY (`stage_code`, `rune_code`)
);

------------------------------------------------------------
-- 스테이지 별 보상 골드 데이터
------------------------------------------------------------
DROP TABLE IF EXISTS `stage_reward_gold`;
CREATE TABLE IF NOT EXISTS `stage_reward_gold` (
    `stage_code` BIGINT NOT NULL PRIMARY KEY    COMMENT '스테이지 식별 코드',
    `gold`       INT    NOT NULL                COMMENT '골드 획득량'
);

------------------------------------------------------------
-- 스테이지 별 몬스터 출현 정보
------------------------------------------------------------
DROP TABLE IF EXISTS `stage_monster_info`;
CREATE TABLE IF NOT EXISTS `stage_monster_info`(
    `stage_code`    BIGINT  NOT NULL    COMMNET `스테이지 식별 코드`,
    `monster_code`  BIGINT  NOT NULL    COMMENT `몬스터 식별 코드`,
    `monster_count` INT     NOT NULL    COMMENT `몬스터 출현 마릿수`,
    PRIMARY KEY (`stage_code`, `monster_code`)
)