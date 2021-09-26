CREATE USER IF NOT EXISTS buttercup_dev@localhost;

GRANT ALL ON buttercup_app.* TO buttercup_dev@localhost;
GRANT ALL ON buttercup_test.* TO buttercup_dev@localhost;
