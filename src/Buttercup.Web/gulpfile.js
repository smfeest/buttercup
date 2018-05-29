const gulp = require('gulp');
const less = require('gulp-less');
const sourcemaps = require('gulp-sourcemaps');
const watchLess = require('gulp-watch-less');

const paths = {};
paths.lessEntryPoint = `${__dirname}/styles/main.less`;
paths.styleAssets = `${__dirname}/wwwroot/assets/styles`;

gulp.task('default', ['build']);

gulp.task('build', ['build:styles']);

gulp.task('build:styles', () => bundleStyles(gulp.src(paths.lessEntryPoint)));

gulp.task('watch', ['watch:styles']);

gulp.task('watch:styles', ['build:styles'], () => bundleStyles(watchLess(paths.lessEntryPoint)));

function bundleStyles(stream) {
  return stream
    .pipe(sourcemaps.init())
    .pipe(less())
    .pipe(sourcemaps.write('./'))
    .pipe(gulp.dest(paths.styleAssets));
}
