module.exports = {
  extends: 'stylelint-config-recommended',
  rules: {
    'no-descending-specificity': [true, { ignore: ['selectors-within-list'] }],
  },
};
