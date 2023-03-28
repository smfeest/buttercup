RENAME TABLE
  authentication_event TO authentication_events,
  password_reset_token TO password_reset_tokens,
  recipe to recipes,
  user TO users;

ALTER TABLE authentication_events
  RENAME INDEX authentication_event_FK1 TO authentication_events_FK1;
ALTER TABLE password_reset_tokens
  RENAME INDEX password_reset_token_FK1 TO password_reset_tokens_FK1;
ALTER TABLE recipes
  RENAME INDEX recipe_FK1 TO recipes_FK1,
  RENAME INDEX recipe_FK2 TO recipes_FK2;
ALTER TABLE users
  RENAME INDEX user_u1 TO users_u1;
