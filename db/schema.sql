CREATE TABLE authentication_events (
  id BIGINT NOT NULL AUTO_INCREMENT,
  time DATETIME NOT NULL,
  event VARCHAR(50) NOT NULL,
  user_id BIGINT,
  email VARCHAR(250),
  PRIMARY KEY (id)
) ENGINE=InnoDB;

CREATE TABLE password_reset_tokens (
  token CHAR(48) NOT NULL,
  user_id BIGINT NOT NULL,
  created DATETIME NOT NULL,
  PRIMARY KEY (token)
) ENGINE=InnoDB;

CREATE TABLE recipes (
  id BIGINT NOT NULL AUTO_INCREMENT,
  title VARCHAR(250) NOT NULL,
  preparation_minutes INT,
  cooking_minutes INT,
  servings INT,
  ingredients TEXT NOT NULL,
  method TEXT NOT NULL,
  suggestions TEXT,
  remarks TEXT,
  source VARCHAR(250),
  created DATETIME NOT NULL,
  created_by_user_id BIGINT,
  modified DATETIME NOT NULL,
  modified_by_user_id BIGINT,
  revision INT NOT NULL,
  PRIMARY KEY (id)
) ENGINE=InnoDB;

CREATE TABLE users (
  id BIGINT NOT NULL AUTO_INCREMENT,
  name VARCHAR(250) NOT NULL,
  email VARCHAR(250) NOT NULL,
  hashed_password VARCHAR(250),
  password_created DATETIME,
  security_stamp CHAR(8) NOT NULL,
  time_zone VARCHAR(50) NOT NULL,
  created DATETIME NOT NULL,
  modified DATETIME NOT NULL,
  revision INT NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY users_u1 (email)
) ENGINE=InnoDB;

ALTER TABLE authentication_events
  ADD FOREIGN KEY authentication_events_FK1 (user_id)
    REFERENCES users (id)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION;

ALTER TABLE password_reset_tokens
  ADD FOREIGN KEY password_reset_tokens_FK1 (user_id)
    REFERENCES users (id)
    ON DELETE CASCADE
    ON UPDATE NO ACTION;

ALTER TABLE recipes
  ADD FOREIGN KEY recipes_FK1 (created_by_user_id)
    REFERENCES users (id)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  ADD FOREIGN KEY recipes_FK2 (modified_by_user_id)
    REFERENCES users (id)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION;
