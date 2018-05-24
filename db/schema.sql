DROP DATABASE IF EXISTS `{DatabaseName}`;
CREATE DATABASE `{DatabaseName}` DEFAULT CHARACTER SET utf8;
USE `{DatabaseName}`;

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
  modified DATETIME NOT NULL,
  revision SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (id)
) ENGINE=InnoDB;
