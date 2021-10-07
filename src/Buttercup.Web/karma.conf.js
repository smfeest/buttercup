module.exports = function (config) {
  config.set({
    browsers: ['ChromeHeadless'],
    frameworks: ['jasmine', 'webpack'],
    preprocessors: {
      '**/*.ts': ['webpack'],
    },
    files: ['scripts/**/*.spec.ts'],
    mime: {
      'text/x-typescript': ['ts'],
    },
    reporters: ['progress', 'kjhtml'],
    webpack: {
      mode: 'development',
      devtool: 'eval-cheap-module-source-map',
      resolve: {
        extensions: ['.js', '.ts'],
      },
      module: {
        rules: [
          {
            test: /\.ts$/,
            use: 'ts-loader',
            exclude: /node_modules/
          },
        ],
      },
      watch: true,
    },
  });
}
