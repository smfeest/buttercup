import cleanCss from 'gulp-clean-css';
import { deleteAsync } from 'del';
import gulp from 'gulp';
import less from 'gulp-less';
import rename from 'gulp-rename';
import webpack from 'webpack';
import webpackStream from 'webpack-stream';

const { dest, parallel, series, src, watch } = gulp;

const paths = {};
paths.scripts = 'scripts';
paths.styles = 'styles';
paths.assets = 'wwwroot/assets';
paths.scriptAssets = `${paths.assets}/scripts`;
paths.styleAssets = `${paths.assets}/styles`;

function bundleProductionScripts() {
  return webpackScripts({
    mode: 'production',
    output: { filename: 'scripts/[name].prod.js' },
  }).pipe(dest(paths.assets));
}

function bundleDevelopmentScripts() {
  return webpackDevScripts().pipe(dest(paths.assets));
}

function bundleStyles() {
  return src(`${paths.styles}/main.less`)
    .pipe(less({ math: 'parens-division' }))
    .pipe(dest(paths.styleAssets))
    .pipe(rename({ suffix: '.prod' }))
    .pipe(cleanCss())
    .pipe(dest(paths.styleAssets));
}

function clean() {
  return deleteAsync([
    `${paths.scriptAssets}/**/*`,
    `${paths.styleAssets}/**/*`,
  ]);
}

function watchScripts() {
  return webpackDevScripts({
    watch: true,
    watchOptions: {
      ignored: /node_modules/,
    },
  }).pipe(dest(paths.assets));
}

function watchStyles() {
  return watch(`${paths.styles}/*.less`, bundleStyles);
}

function webpackDevScripts(config) {
  return webpackScripts({
    mode: 'development',
    devtool: 'eval-cheap-module-source-map',
    output: { filename: 'scripts/[name].js' },
    ...config,
  });
}

function webpackScripts(config) {
  return src(`${paths.scripts}/main.ts`).pipe(
    webpackStream(
      {
        resolve: {
          extensions: ['.js', '.ts'],
        },
        module: {
          rules: [
            {
              test: /\.ts$/,
              use: 'ts-loader',
              exclude: /node_modules/,
            },
          ],
        },
        ...config,
      },
      webpack,
    ),
  );
}

const build = parallel(
  bundleDevelopmentScripts,
  bundleProductionScripts,
  bundleStyles,
);

const rebuild = series(clean, build);

const watchAll = parallel(bundleStyles, watchScripts, watchStyles);

export { build as default, build, clean, rebuild, watchAll as watch };
