module.exports = {
  extends: 'stylelint-config-recommended',
  customSyntax: 'postcss-less',
  rules: {
    'at-rule-prelude-no-invalid': null,
    'declaration-property-value-no-unknown': null,
    'function-no-unknown': [true, { ignoreFunctions: ['darken', 'lighten'] }],
    'media-query-no-invalid': null,
    'no-descending-specificity': [true, { ignore: ['selectors-within-list'] }],
  },
};
