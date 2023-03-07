import cleanCss from 'gulp-clean-css';
import { deleteAsync } from 'del';
import gulp from 'gulp';
import less from 'gulp-less';
import rev from 'gulp-rev';
import revReplace from 'gulp-rev-replace';
import sourcemaps from 'gulp-sourcemaps';
import webpack from 'webpack';
import webpackStream from 'webpack-stream';

const { dest, parallel, series, src, watch } = gulp;

const paths = {};
paths.scripts = 'scripts';
paths.styles = 'styles';
paths.assets = 'wwwroot/assets';
paths.scriptAssets = `${paths.assets}/scripts`;
paths.styleAssets = `${paths.assets}/styles`;
paths.prodAssets = 'wwwroot/prod-assets';
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
  return deleteAsync([
    `${paths.scriptAssets}/**/*`,
    `${paths.styleAssets}/**/*`,
    `${paths.prodAssets}/**/*`,
  ]);
}

function revisionAssetsInStream(stream) {
  return stream
    .pipe(rev())
    .pipe(dest(paths.prodAssets))
    .pipe(
      rev.manifest(paths.prodAssetManifest, {
        base: paths.assets,
        merge: true,
      })
    )
    .pipe(dest(paths.prodAssets));
}

function revisionStaticAssets() {
  return revisionAssetsInStream(
    src(`${paths.assets}/{images,fonts}/**/*`, { base: paths.assets })
  );
}

function revisionStyles() {
  return revisionAssetsInStream(
    src(`${paths.styleAssets}/*.css`, { base: paths.assets })
      .pipe(revReplace({ manifest: src(paths.prodAssetManifest) }))
      .pipe(cleanCss())
  );
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
        output: { filename: 'scripts/[name].js' },
        ...config,
      },
      webpack
    )
  );
}

const build = parallel(
  bundleDevelopmentScripts,
  series(
    parallel(bundleStyles, revisionStaticAssets),
    bundleAndRevisionProductionScripts,
    revisionStyles
  )
);

const rebuild = series(clean, build);

const watchAll = parallel(bundleStyles, watchScripts, watchStyles);

export { build as default, build, clean, rebuild, watchAll as watch };
