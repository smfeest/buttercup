
const cleanCss = require('gulp-clean-css');
const del = require('del');
const { dest, parallel, series, src, watch } = require('gulp');
const karma = require('karma');
const less = require('gulp-less');
const lesshint = require('gulp-lesshint');
const rev = require('gulp-rev');
const revReplace = require('gulp-rev-replace');
const sourcemaps = require('gulp-sourcemaps');
const tslint = require('gulp-tslint');
const webpack = require('webpack');
const webpackStream = require('webpack-stream');

const paths = {};
paths.scripts = `${__dirname}/scripts`;
paths.styles = `${__dirname}/styles`;
paths.assets = `${__dirname}/wwwroot/assets`;
paths.scriptAssets = `${paths.assets}/scripts`;
paths.styleAssets = `${paths.assets}/styles`;
paths.prodAssets = `${__dirname}/wwwroot/prod-assets`;
paths.prodAssetManifest = `${paths.prodAssets}/manifest.json`;

function bundleAndRevisionProductionScripts() {
  return revisionAssetsInStream(webpackScripts({ mode: 'production' }));
}

function bundleDevelopmentScripts() {
  return webpackDevScripts().pipe(dest(paths.assets));
}

function bundleStyles() {
  return src(`${paths.styles}/{main,print}.less`)
    .pipe(sourcemaps.init())
    .pipe(less({ math: 'parens-division' }))
    .pipe(sourcemaps.write('./'))
    .pipe(dest(paths.styleAssets));
}

function clean() {
  return del([
    `${paths.scriptAssets}/**/*`,
    `${paths.styleAssets}/**/*`,
    `${paths.prodAssets}/**/*`,
  ]);
}

function lintScripts() {
  return src(`${paths.scripts}/**/*.ts`)
    .pipe(tslint({ formatter: "verbose" }))
    .pipe(tslint.report());
}

function lintStyles() {
  return src(`${paths.styles}/*.less`)
    .pipe(lesshint())
    .pipe(lesshint.reporter())
    .pipe(lesshint.failOnError());
}

function revisionAssetsInStream(stream) {
  return stream
    .pipe(rev())
    .pipe(dest(paths.prodAssets))
    .pipe(rev.manifest(paths.prodAssetManifest, {
      base: paths.assets,
      merge: true,
    }))
    .pipe(dest(paths.prodAssets));
}

function revisionStaticAssets() {
  return revisionAssetsInStream(src(`${paths.assets}/{images,fonts}/**/*`, { base: paths.assets }));
}

function revisionStyles() {
  return revisionAssetsInStream(src(`${paths.styleAssets}/*.css`, { base: paths.assets })
    .pipe(revReplace({ manifest: src(paths.prodAssetManifest) }))
    .pipe(cleanCss()));
}

function test(browser) {
  return doneCallback => {
    const config = karma.config.parseConfig(`${__dirname}/karma.conf.js`, {
      browsers: [browser]
    });
    new karma.Server(config, doneCallback).start();
  };
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
    ...config
  });
}

function webpackScripts(config) {
  return src(`${paths.scripts}/main.ts`).pipe(webpackStream({
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
    output: { filename: 'scripts/[name].js' },
    ...config,
  }, webpack));
}

const lint = parallel(lintScripts, lintStyles);

const build = parallel(
  bundleDevelopmentScripts,
  series(
    parallel(bundleStyles, revisionStaticAssets),
    bundleAndRevisionProductionScripts,
    revisionStyles));

exports.default = build;
exports.build = build;
exports.clean = clean;
exports.lint = lint;
exports.rebuild = series(clean, build);
exports.test = test('ChromeHeadless');
exports.testDebug = test('Chrome');
exports.watch = parallel(bundleStyles, watchScripts, watchStyles);
