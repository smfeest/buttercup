DROP DATABASE IF EXISTS `{DatabaseName}`;
CREATE DATABASE `{DatabaseName}` DEFAULT CHARACTER SET utf8;
USE `{DatabaseName}`;

CREATE TABLE authentication_event (
  id INT UNSIGNED NOT NULL AUTO_INCREMENT,
  time DATETIME NOT NULL,
  event VARCHAR(50) NOT NULL,
  user_id INT UNSIGNED,
  email VARCHAR(250),
  PRIMARY KEY (id)
) ENGINE=InnoDB;

CREATE TABLE password_reset_token (
  token CHAR(48) NOT NULL,
  user_id INT UNSIGNED NOT NULL,
  created DATETIME NOT NULL,
  PRIMARY KEY (token)
) ENGINE=InnoDB;

CREATE TABLE recipe (
  id INT UNSIGNED NOT NULL AUTO_INCREMENT,
  title VARCHAR(255) NOT NULL,
  preparation_minutes SMALLINT UNSIGNED,
  cooking_minutes SMALLINT UNSIGNED,
  servings SMALLINT UNSIGNED,
  ingredients TEXT NOT NULL,
  method TEXT NOT NULL,
  suggestions TEXT,
  remarks TEXT,
  source VARCHAR(255),
  created DATETIME NOT NULL,
  created_by_user_id INT UNSIGNED,
  modified DATETIME NOT NULL,
  modified_by_user_id INT UNSIGNED,
  revision SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (id)
) ENGINE=InnoDB;

CREATE TABLE user (
  id INT UNSIGNED NOT NULL AUTO_INCREMENT,
  name VARCHAR(250) NOT NULL,
  email VARCHAR(250) NOT NULL,
  hashed_password CHAR(84),
  password_created DATETIME,
  security_stamp CHAR(8) NOT NULL,
  time_zone VARCHAR(50) NOT NULL,
  created DATETIME NOT NULL,
  modified DATETIME NOT NULL,
  revision SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (id),
  UNIQUE KEY user_u1 (email)
) ENGINE=InnoDB;

ALTER TABLE authentication_event
  ADD FOREIGN KEY authentication_event_FK1 (user_id)
    REFERENCES user (id)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION;

ALTER TABLE password_reset_token
  ADD FOREIGN KEY password_reset_token_FK1 (user_id)
    REFERENCES user (id)
    ON DELETE CASCADE
    ON UPDATE NO ACTION;

ALTER TABLE recipe
  ADD FOREIGN KEY recipe_FK1 (created_by_user_id)
    REFERENCES user (id)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  ADD FOREIGN KEY recipe_FK2 (modified_by_user_id)
    REFERENCES user (id)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION;
