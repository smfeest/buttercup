module.exports = {
  extends: 'stylelint-config-recommended',
  customSyntax: 'postcss-less',
  rules: {
    'no-descending-specificity': [true, { ignore: ['selectors-within-list'] }],
  },
};
