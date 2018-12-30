UPDATE user
SET password_created = modified
WHERE hashed_password IS NOT NULL AND password_created IS NULL;
