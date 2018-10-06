module.exports = function (config) {
  config.set({
    browsers: ['ChromeHeadless'],
    frameworks: ['jasmine'],
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
      devtool: 'cheap-module-eval-source-map',
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