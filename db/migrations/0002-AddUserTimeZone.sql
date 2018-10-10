ALTER TABLE user ADD time_zone VARCHAR(50) DEFAULT 'Etc/UTC' AFTER security_stamp;
ALTER TABLE user ALTER time_zone DROP DEFAULT;
