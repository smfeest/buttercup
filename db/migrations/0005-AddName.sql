ALTER TABLE user ADD name VARCHAR(250) NOT NULL DEFAULT 'Buttercup User' AFTER id;
ALTER TABLE user ALTER name DROP DEFAULT;