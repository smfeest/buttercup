ALTER TABLE authentication_event
  DROP FOREIGN KEY authentication_event_ibfk_1;
ALTER TABLE password_reset_token
  DROP FOREIGN KEY password_reset_token_ibfk_1;
ALTER TABLE recipe
  DROP FOREIGN KEY recipe_ibfk_1,
  DROP FOREIGN KEY recipe_ibfk_2;

ALTER TABLE authentication_event
  MODIFY COLUMN id BIGINT NOT NULL AUTO_INCREMENT,
  MODIFY COLUMN user_id BIGINT;
ALTER TABLE password_reset_token
  MODIFY COLUMN user_id BIGINT NOT NULL;
ALTER TABLE recipe
  MODIFY COLUMN id BIGINT NOT NULL AUTO_INCREMENT,
  MODIFY COLUMN preparation_minutes INT,
  MODIFY COLUMN cooking_minutes INT,
  MODIFY COLUMN servings INT,
  MODIFY COLUMN created_by_user_id BIGINT,
  MODIFY COLUMN modified_by_user_id BIGINT,
  MODIFY COLUMN revision INT NOT NULL;
ALTER TABLE user
  MODIFY COLUMN id BIGINT NOT NULL AUTO_INCREMENT,
  MODIFY COLUMN revision INT NOT NULL;

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
