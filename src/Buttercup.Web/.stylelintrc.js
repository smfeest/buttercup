module.exports = {
  extends: 'stylelint-config-recommended',
  customSyntax: 'postcss-less',
  rules: {
    'function-no-unknown': [true, { ignoreFunctions: ['darken', 'lighten'] }],
    'media-query-no-invalid': null,
    'no-descending-specificity': [true, { ignore: ['selectors-within-list'] }],
  },
};
