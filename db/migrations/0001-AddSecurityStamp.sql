ALTER TABLE user ADD security_stamp CHAR(8) AFTER hashed_password;
UPDATE user SET security_stamp = LEFT(TO_BASE64(UNHEX(SHA2(CONCAT(email, RAND()), 256))), 8);
ALTER TABLE user MODIFY security_stamp CHAR(8) NOT NULL;
