const gulp = require('gulp');
const less = require('gulp-less');
const lesshint = require('gulp-lesshint');
const sourcemaps = require('gulp-sourcemaps');
const watchLess = require('gulp-watch-less');

const paths = {};
paths.styles = `${__dirname}/styles`;
paths.lessEntryPoint = `${paths.styles}/main.less`;
paths.styleAssets = `${__dirname}/wwwroot/assets/styles`;

gulp.task('default', ['build']);

gulp.task('build', ['build:styles', 'lint:styles']);

gulp.task('build:styles', () => bundleStyles(gulp.src(paths.lessEntryPoint)));

gulp.task('lint', ['lint:styles']);

gulp.task('lint:styles', () => gulp.src(`${paths.styles}/*.less`)
  .pipe(lesshint())
  .pipe(lesshint.reporter())
  .pipe(lesshint.failOnError()));

gulp.task('watch', ['watch:styles']);

gulp.task('watch:styles', ['build:styles'], () => bundleStyles(watchLess(paths.lessEntryPoint)));

function bundleStyles(stream) {
  return stream
    .pipe(sourcemaps.init())
    .pipe(less())
    .pipe(sourcemaps.write('./'))
    .pipe(gulp.dest(paths.styleAssets));
}
