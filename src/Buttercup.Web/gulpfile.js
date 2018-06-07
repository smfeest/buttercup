
const cleanCss = require('gulp-clean-css');
const gulp = require('gulp');
const less = require('gulp-less');
const lesshint = require('gulp-lesshint');
const rev = require('gulp-rev');
const revReplace = require('gulp-rev-replace');
const sourcemaps = require('gulp-sourcemaps');
const watchLess = require('gulp-watch-less');

const paths = {};
paths.styles = `${__dirname}/styles`;
paths.lessEntryPoint = `${paths.styles}/main.less`;
paths.assets = `${__dirname}/wwwroot/assets`;
paths.styleAssets = `${paths.assets}/styles`;
paths.prodAssets = `${__dirname}/wwwroot/prod-assets`;
paths.prodAssetManifest = `${paths.prodAssets}/manifest.json`;

gulp.task('default', ['build']);

gulp.task('build', ['build:staticAssets', 'build:styles']);

gulp.task('build:staticAssets', ['build:staticAssets:prod']);

gulp.task('build:staticAssets:prod',
  () => revAssets(gulp.src(`${paths.assets}/{images,fonts}/**/*`, { base: paths.assets })));

gulp.task('build:styles', ['build:styles:dev', 'build:styles:prod', 'lint:styles']);

gulp.task('build:styles:dev', () => bundleStyles(gulp.src(paths.lessEntryPoint)));

gulp.task('build:styles:prod', ['build:styles:dev', 'build:staticAssets:prod'],
  () => revAssets(gulp.src(`${paths.styleAssets}/main.css`, { base: paths.assets })
    .pipe(revReplace({ manifest: gulp.src(paths.prodAssetManifest) }))
    .pipe(cleanCss())));

gulp.task('lint', ['lint:styles']);

gulp.task('lint:styles', () => gulp.src(`${paths.styles}/*.less`)
  .pipe(lesshint())
  .pipe(lesshint.reporter())
  .pipe(lesshint.failOnError()));

gulp.task('watch', ['watch:styles']);

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
