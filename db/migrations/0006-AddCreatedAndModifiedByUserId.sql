ALTER TABLE recipe
  ADD created_by_user_id INT UNSIGNED AFTER created,
  ADD modified_by_user_id INT UNSIGNED AFTER modified,
  ADD FOREIGN KEY recipe_FK1 (created_by_user_id)
    REFERENCES user (id)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  ADD FOREIGN KEY recipe_FK2 (modified_by_user_id)
    REFERENCES user (id)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION;
