module.exports = {
  extends: 'stylelint-config-recommended',
  customSyntax: 'postcss-less',
  rules: {
    'function-no-unknown': [true, { ignoreFunctions: ['darken', 'lighten'] }],
    'no-descending-specificity': [true, { ignore: ['selectors-within-list'] }],
  },
};
