START TRANSACTION;

ALTER TABLE `recipes` ADD `deleted` datetime(6) NULL;

ALTER TABLE `recipes` ADD `deleted_by_user_id` bigint NULL;

CREATE INDEX `ix_recipes_deleted` ON `recipes` (`deleted`);

CREATE INDEX `ix_recipes_deleted_by_user_id` ON `recipes` (`deleted_by_user_id`);

ALTER TABLE `recipes` ADD CONSTRAINT `fk_recipes_users_deleted_by_user_id` FOREIGN KEY (`deleted_by_user_id`) REFERENCES `users` (`id`);

INSERT INTO `__migrations_history` (`migration_id`, `product_version`)
VALUES ('20240329140226_AddDeletedAndDeletedByUserToRecipe', '8.0.2');

COMMIT;

START TRANSACTION;

CREATE TABLE `recipe_revisions` (
    `recipe_id` bigint NOT NULL,
    `revision` int NOT NULL,
    `created` datetime(6) NOT NULL,
    `created_by_user_id` bigint NULL,
    `title` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `preparation_minutes` int NULL,
    `cooking_minutes` int NULL,
    `servings` int NULL,
    `ingredients` text CHARACTER SET utf8mb4 NOT NULL,
    `method` text CHARACTER SET utf8mb4 NOT NULL,
    `suggestions` text CHARACTER SET utf8mb4 NULL,
    `remarks` text CHARACTER SET utf8mb4 NULL,
    `source` varchar(250) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `pk_recipe_revisions` PRIMARY KEY (`recipe_id`, `revision`),
    CONSTRAINT `fk_recipe_revisions_recipes_recipe_id` FOREIGN KEY (`recipe_id`) REFERENCES `recipes` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_recipe_revisions_users_created_by_user_id` FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`id`)
) CHARACTER SET=utf8mb4;

CREATE INDEX `ix_recipe_revisions_created_by_user_id` ON `recipe_revisions` (`created_by_user_id`);

INSERT INTO `__migrations_history` (`migration_id`, `product_version`)
VALUES ('20240407195924_AddRecipeRevisions', '8.0.2');

COMMIT;

