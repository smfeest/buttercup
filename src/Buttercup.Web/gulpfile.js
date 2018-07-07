
const cleanCss = require('gulp-clean-css');
const del = require('del');
const gulp = require('gulp');
const less = require('gulp-less');
const lesshint = require('gulp-lesshint');
const rev = require('gulp-rev');
const revReplace = require('gulp-rev-replace');
const sourcemaps = require('gulp-sourcemaps');
const watchLess = require('gulp-watch-less');
const webpack = require('webpack');
const webpackStream = require('webpack-stream');

const paths = {};
paths.scripts = `${__dirname}/scripts`;
paths.styles = `${__dirname}/styles`;
paths.lessEntryPoint = `${paths.styles}/{main,print}.less`;
paths.assets = `${__dirname}/wwwroot/assets`;
paths.scriptAssets = `${paths.assets}/scripts`;
paths.styleAssets = `${paths.assets}/styles`;
paths.prodAssets = `${__dirname}/wwwroot/prod-assets`;
paths.prodAssetManifest = `${paths.prodAssets}/manifest.json`;

gulp.task('default', ['build']);

gulp.task('build', ['build:scripts', 'build:staticAssets', 'build:styles']);

gulp.task('build:scripts', ['build:scripts:dev', 'build:scripts:prod']);

gulp.task('build:scripts:dev', () => webpackDevScripts().pipe(gulp.dest(paths.assets)));

gulp.task('build:scripts:prod', ['build:staticAssets:prod'],
  () => revAssets(webpackScripts({ mode: 'production' })));

gulp.task('build:staticAssets', ['build:staticAssets:prod']);

gulp.task('build:staticAssets:prod',
  () => revAssets(gulp.src(`${paths.assets}/{images,fonts}/**/*`, { base: paths.assets })));

gulp.task('build:styles', ['build:styles:dev', 'build:styles:prod', 'lint:styles']);

gulp.task('build:styles:dev', () => bundleStyles(gulp.src(paths.lessEntryPoint)));

gulp.task('build:styles:prod', ['build:styles:dev', 'build:scripts:prod'],
  () => revAssets(gulp.src(`${paths.styleAssets}/*.css`, { base: paths.assets })
    .pipe(revReplace({ manifest: gulp.src(paths.prodAssetManifest) }))
    .pipe(cleanCss())));

gulp.task('clean', () => del([
  `${paths.scriptAssets}/**/*`,
  `${paths.styleAssets}/**/*`,
  `${paths.prodAssets}/**/*`,
]));

gulp.task('lint', ['lint:styles']);

gulp.task('lint:styles', () => gulp.src(`${paths.styles}/*.less`)
  .pipe(lesshint())
  .pipe(lesshint.reporter())
  .pipe(lesshint.failOnError()));

gulp.task('watch', ['watch:scripts', 'watch:styles']);

gulp.task('watch:scripts', () => webpackDevScripts({
  watch: true,
  watchOptions: {
    ignored: /node_modules/,
  },
}).pipe(gulp.dest(paths.assets)));

gulp.task('watch:styles', ['build:styles'], () => bundleStyles(watchLess(paths.lessEntryPoint)));

gulp.task('lint:styles', () => gulp.src(`${paths.styles}/*.less`)
  .pipe(lesshint())
  .pipe(lesshint.reporter())
  .pipe(lesshint.failOnError()));

function bundleStyles(stream) {
  return stream
    .pipe(sourcemaps.init())
    .pipe(less())
    .pipe(sourcemaps.write('./'))
    .pipe(gulp.dest(paths.styleAssets));
}

function revAssets(stream) {
  return stream
    .pipe(rev())
    .pipe(gulp.dest(paths.prodAssets))
    .pipe(rev.manifest(paths.prodAssetManifest, {
      base: paths.assets,
      merge: true,
    }))
    .pipe(gulp.dest(paths.prodAssets));
}

function webpackDevScripts(config) {
  return webpackScripts({
    mode: 'development',
    devtool: 'cheap-module-eval-source-map',
    ...config
  });
}

function webpackScripts(config) {
  return gulp.src(`${paths.scripts}/main.ts`).pipe(webpackStream({
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
