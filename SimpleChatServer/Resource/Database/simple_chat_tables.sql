CREATE TABLE IF NOT EXISTS users
(
    id              BIGINT PRIMARY KEY AUTO_INCREMENT,
    username        VARCHAR(16),
    hashed_password VARCHAR(64) NOT NULL,-- SHA-256 hashed value
    password_salt   VARCHAR(32) NOT NULL,
    email           VARCHAR(128)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS verification_codes
(
    id        BIGINT PRIMARY KEY AUTO_INCREMENT,
    email     VARCHAR(128) NOT NULL,
    code      VARCHAR(8)   NOT NULL,
    send_time DATETIME     NOT NULL
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS chats
(
    id    BIGINT PRIMARY KEY AUTO_INCREMENT,
    title VARCHAR(32)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS chat_messages
(
    id             BIGINT PRIMARY KEY AUTO_INCREMENT,
    chat_id        BIGINT REFERENCES chats (id),
    content        TEXT     NOT NULL,
    sender_id      BIGINT REFERENCES users (id),
    send_date_time DATETIME NOT NULL,
    type           INT      NOT NULL DEFAULT 0
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS users_in_chats
(
    id      BIGINT PRIMARY KEY AUTO_INCREMENT,
    chat_id BIGINT REFERENCES chats (id),
    user_id BIGINT REFERENCES users (id)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS chat_images
(
    id      BIGINT PRIMARY KEY AUTO_INCREMENT,
    chat_id BIGINT REFERENCES chats (id),
    path    TEXT NOT NULL
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;